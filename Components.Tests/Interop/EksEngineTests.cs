﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Interop.TestData;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Serilog.Extensions.Logging;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Interop
{
    /// <summary>
    /// Tests the command sequence for:
    /// Fake inbound IKS in DB
    /// Snapshot to DK Source
    /// Snapshot for EKS
    /// Build EKS
    /// </summary>
    public abstract class EksEngineTests : IDisposable
    {
        private readonly ILoggerFactory _LoggerFactory = new SerilogLoggerFactory();
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider = new StandardUtcDateTimeProvider();
        private readonly StandardRandomNumberGenerator _Rng = new StandardRandomNumberGenerator();

        private readonly IDbProvider<IksInDbContext> _IksInDbContextProvider;
        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbContextProvider;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbContextProvider;
        private readonly IDbProvider<EksPublishingJobDbContext> _PublishingJobDbContextProvider;
        private readonly IDbProvider<ContentDbContext> _ContentDbContextProvider;

        private readonly IWrappedEfExtensions _EfExtensions;
        private const int EksMinTekCount = 150;
        private const int TekCountMax = 750;

        protected EksEngineTests(IDbProvider<IksInDbContext> iksInDbContextProvider, IDbProvider<WorkflowDbContext> workflowDbContextProvider, IDbProvider<DkSourceDbContext> dkSourceDbContextProvider, IDbProvider<EksPublishingJobDbContext> publishingJobDbContextProvider, IDbProvider<ContentDbContext> contentDbContextProvider, IWrappedEfExtensions efExtensions)
        {
            _IksInDbContextProvider = iksInDbContextProvider ?? throw new ArgumentNullException(nameof(iksInDbContextProvider));
            _WorkflowDbContextProvider = workflowDbContextProvider ?? throw new ArgumentNullException(nameof(workflowDbContextProvider));
            _DkSourceDbContextProvider = dkSourceDbContextProvider ?? throw new ArgumentNullException(nameof(dkSourceDbContextProvider));
            _PublishingJobDbContextProvider = publishingJobDbContextProvider ?? throw new ArgumentNullException(nameof(publishingJobDbContextProvider));
            _ContentDbContextProvider = contentDbContextProvider ?? throw new ArgumentNullException(nameof(contentDbContextProvider));
            _EfExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));
        }
        

        private ExposureKeySetBatchJobMk3 Create()
        {
            //Mocks
            var eksConfigMock = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfigMock.Setup(x => x.LifetimeDays).Returns(14);
            eksConfigMock.Setup(x => x.TekCountMin).Returns(EksMinTekCount);
            eksConfigMock.Setup(x => x.TekCountMax).Returns(TekCountMax);
            eksConfigMock.Setup(x => x.PageSize).Returns(1000);

            var eksHeaderInfoMock = new Mock<IEksHeaderInfoConfig>(MockBehavior.Strict);
            eksHeaderInfoMock.Setup(x => x.AppBundleId).Returns("FakeAppBundleId");
            eksHeaderInfoMock.Setup(x => x.VerificationKeyId).Returns("FakeVerificationKeyId");
            eksHeaderInfoMock.Setup(x => x.VerificationKeyVersion).Returns("FakeVerificationKeyVersion");

            var tekValidatorConfigMock = new Mock<ITekValidatorConfig>(MockBehavior.Loose);
            tekValidatorConfigMock.Setup(x => x.RollingPeriodMax).Returns(UniversalConstants.RollingPeriodMax);
            tekValidatorConfigMock.Setup(x => x.MaxAgeDays).Returns(14);
            tekValidatorConfigMock.Setup(x => x.KeyDataLength).Returns(30);

            //Real objects:
            var lf = new LoggerFactory();
            var dtp = new StandardUtcDateTimeProvider();


            var le = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            var s1 = new EcdSaSigner(
                new EmbeddedResourceCertificateProvider(
                    new HardCodedCertificateLocationConfig("TestECDSA.p12", ""),
                    le));

            var s2 = new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(
                    new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"),
                    le),
                new EmbeddedResourcesCertificateChainProvider(new HardCodedCertificateLocationConfig("StaatDerNLChain-Expires2020-08-28.p7b", "")),
                dtp);

            var sut = new EksBuilderV1(
                eksHeaderInfoMock.Object,
                s1,
                s2,
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                new EksBuilderV1LoggingExtensions(lf.CreateLogger<EksBuilderV1LoggingExtensions>())
            );

            var eksJob = new ExposureKeySetBatchJobMk3(
                eksConfigMock.Object,
                sut,
                _PublishingJobDbContextProvider.CreateNew,
                _UtcDateTimeProvider,
                new EksEngineLoggingExtensions(_LoggerFactory.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), _Rng, _UtcDateTimeProvider, eksConfigMock.Object),
                new SnapshotDiagnosisKeys(_LoggerFactory.CreateLogger<SnapshotDiagnosisKeys>(), _DkSourceDbContextProvider.CreateNew(), _PublishingJobDbContextProvider.CreateNew),
                new MarkDiagnosisKeysAsUsedLocally(_DkSourceDbContextProvider.CreateNew, eksConfigMock.Object, _PublishingJobDbContextProvider.CreateNew, _LoggerFactory.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_ContentDbContextProvider.CreateNew, _PublishingJobDbContextProvider.CreateNew, new Sha256HexPublishingIdService(), new EksJobContentWriterLoggingExtensions(_LoggerFactory.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_DkSourceDbContextProvider.CreateNew(), _PublishingJobDbContextProvider.CreateNew()),
                _EfExtensions
            );
            return eksJob;
        }

        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task Empty()
        {
            var result2 = await Create().ExecuteAsync();
            Assert.Equal(0, result2.InputCount);
            Assert.Equal(0, result2.StuffingCount);
            Assert.Equal(0, result2.OutputCount);
            Assert.Equal(0, result2.EksInfo.Length);
            Assert.Equal(0, result2.ReconcileEksSumCount);
            Assert.Equal(0, result2.ReconcileOutputCount);
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
        }

        
        /// <summary>
        /// End-to-end test for consuming workflows
        /// Randomisation of workflows includes items that will be filtered out cos TRL=None so use dkCount for asserts.
        /// </summary>
        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task Execute2RunsFromWorkflows()
        {
            var wfCount = 25;
            var tekPerWfCount = 14;

            var dkCount = await new WorkflowTestDataGenerator(
                _WorkflowDbContextProvider,
                _DkSourceDbContextProvider,
                _EfExtensions
                ).GenerateAndAuthoriseWorkflowsAsync(wfCount, tekPerWfCount);

            Assert.True(dkCount > 0);

            var result = await Create().ExecuteAsync();
            Assert.Equal(dkCount, result.InputCount);

            var expectedStuff = dkCount > EksMinTekCount ? 0 : EksMinTekCount - dkCount;

            Assert.Equal(expectedStuff, result.StuffingCount);
            Assert.Equal(dkCount, result.OutputCount);
            Assert.Equal(1, result.EksInfo.Length);
            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);
            var eksResult = result.EksInfo[0];
            Assert.Equal(dkCount, eksResult.TekCount);
            Assert.Equal(dkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));

            //Second run does not produce anything
            var result2 = await Create().ExecuteAsync();
            Assert.Equal(0, result2.InputCount);
            Assert.Equal(0, result2.StuffingCount);
            Assert.Equal(0, result2.OutputCount);
            Assert.Equal(0, result2.EksInfo.Length);
            Assert.Equal(0, result2.ReconcileEksSumCount);
            Assert.Equal(0, result2.ReconcileOutputCount);
            Assert.Equal(dkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));
        }

        /// <summary>
        /// End-to-end test for consuming IKS from EFGS
        /// </summary>
        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task ExecuteFromIks()
        {
            var dkCount = new IksTestDataGenerator(_IksInDbContextProvider).CreateIks(2);

            var acceptableCountriesSettingMock = new Mock<IAcceptableCountriesSetting>(MockBehavior.Strict);
            acceptableCountriesSettingMock.Setup(x => x.AcceptableCountries).Returns(new []{"DE"});

            await new IksImportBatchJob(_UtcDateTimeProvider, _IksInDbContextProvider.CreateNew(),
                () => new IksImportCommand(_DkSourceDbContextProvider.CreateNew(), new IDiagnosticKeyProcessor[] {
                    //Current prod config
                    new OnlyIncludeCountryOfOriginKeyProcessor(acceptableCountriesSettingMock.Object),
                    new DosDecodingDiagnosticKeyProcessor(),
                    new NlTrlFromDecodedDosDiagnosticKeyProcessor(new TransmissionRiskLevelCalculationMk2()),
                    new ExcludeTrlNoneDiagnosticKeyProcessor(),
                })
            ).ExecuteAsync();

            Assert.Equal(dkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(dkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));

            var result = await Create().ExecuteAsync();
            Assert.Equal(dkCount, result.InputCount);
            Assert.Equal(EksMinTekCount, result.StuffingCount + dkCount);
            Assert.Equal(EksMinTekCount, result.OutputCount);
            Assert.Equal(1, result.EksInfo.Length);
            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);
            var eksResult = result.EksInfo[0];
            Assert.Equal(EksMinTekCount, eksResult.TekCount);
            Assert.Equal(EksMinTekCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));

            //Second run does not produce anything
            var result2 = await Create().ExecuteAsync();
            Assert.Equal(0, result2.InputCount);
            Assert.Equal(0, result2.StuffingCount);
            Assert.Equal(0, result2.OutputCount);
            Assert.Equal(0, result2.EksInfo.Length);
            Assert.Equal(0, result2.ReconcileEksSumCount);
            Assert.Equal(0, result2.ReconcileOutputCount);
            Assert.Equal(EksMinTekCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));
        }

        public void Dispose()
        {
            _LoggerFactory?.Dispose();
            _IksInDbContextProvider?.Dispose();
            _WorkflowDbContextProvider?.Dispose();
            _DkSourceDbContextProvider?.Dispose();
            _PublishingJobDbContextProvider?.Dispose();
            _ContentDbContextProvider?.Dispose();
        }
    }
}
