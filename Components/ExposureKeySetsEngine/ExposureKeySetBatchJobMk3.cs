﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    /// <summary>
    /// Add database IO to the job
    /// </summary>
    public sealed class ExposureKeySetBatchJobMk3
    {
        private readonly IEksConfig _EksConfig;
        private readonly IEksBuilder _SetBuilder;
        private readonly IEksStuffingGenerator _EksStuffingGenerator;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger _Logger;
        private readonly ISnapshotEksInput _Snapshotter;
        private readonly IMarkWorkFlowTeksAsUsed _MarkWorkFlowTeksAsUsed;

        private readonly EksJobContentWriter _ContentWriter;

        private readonly string _JobName;
        private readonly EksEngineResult _EksEngineResult = new EksEngineResult();
        private readonly IList<EksInfo> _EksResults = new List<EksInfo>();

        private readonly List<EksCreateJobInputEntity> _Output;
        private int _EksCount;

        private bool _Fired;
        private readonly Stopwatch _BuildEksStopwatch = new Stopwatch();
        private readonly Func<PublishingJobDbContext> _PublishingDbContextFac;

        public ExposureKeySetBatchJobMk3(IEksConfig eksConfig, IEksBuilder builder, Func<PublishingJobDbContext> publishingDbContextFac, IUtcDateTimeProvider dateTimeProvider, ILogger<ExposureKeySetBatchJobMk3> logger, IEksStuffingGenerator eksStuffingGenerator, ISnapshotEksInput snapshotter, IMarkWorkFlowTeksAsUsed markWorkFlowTeksAsUsed, EksJobContentWriter contentWriter)
        {
            //_JobConfig = jobConfig;
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
            _SetBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _PublishingDbContextFac = publishingDbContextFac ?? throw new ArgumentNullException(nameof(publishingDbContextFac));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _EksStuffingGenerator = eksStuffingGenerator ?? throw new ArgumentNullException(nameof(eksStuffingGenerator));
            _Snapshotter = snapshotter;
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _MarkWorkFlowTeksAsUsed = markWorkFlowTeksAsUsed ?? throw new ArgumentNullException(nameof(markWorkFlowTeksAsUsed));
            _ContentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
            _Output = new List<EksCreateJobInputEntity>(_EksConfig.TekCountMax);
            _JobName = $"ExposureKeySetsJob_{_DateTimeProvider.Snapshot:u}".Replace(" ", "_").Replace(":", "_");
        }

        public async Task<EksEngineResult> Execute()
        {
            if (_Fired)
                throw new InvalidOperationException("One use only.");

            _Fired = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _Logger.LogInformation("Started - JobName:{JobName}", _JobName);

            if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
                _Logger.LogWarning("{JobName} started WITHOUT elevated privileges - errors may occur when signing content.", _JobName);

            _EksEngineResult.Started = _DateTimeProvider.Snapshot; //Align with the logged job name.

            await ClearJobTables();

            var snapshotResult = await _Snapshotter.Execute(_EksEngineResult.Started);
            
            _EksEngineResult.InputCount = snapshotResult.TekInputCount;
            _EksEngineResult.SnapshotSeconds = snapshotResult.SnapshotSeconds;
            _EksEngineResult.TransmissionRiskNoneCount = await GetTransmissionRiskNoneCount();

            if (snapshotResult.TekInputCount != 0)
            {
                await Stuff();
                await BuildOutput();
                await CommitResults();
            }

            _EksEngineResult.TotalSeconds = stopwatch.Elapsed.TotalSeconds;
            _EksEngineResult.EksInfo = _EksResults.ToArray();

            _Logger.LogInformation("Reconciliation - Teks in EKSs matches usable input and stuffing - Delta:{ReconcileOutputCount}", _EksEngineResult.ReconcileOutputCount);
            _Logger.LogInformation("Reconciliation - Teks in EKSs matches output count - Delta:{ReconcileEksSumCount}", _EksEngineResult.ReconcileEksSumCount);

            _Logger.LogInformation("{JobName} complete.", _JobName);

            return _EksEngineResult;
        }

        private async Task<int> GetTransmissionRiskNoneCount()
        {
            await using var dbc = _PublishingDbContextFac();
            return dbc.EksInput.Count(x => x.TransmissionRiskLevel == TransmissionRiskLevel.None);
        }

        private async Task ClearJobTables()
        {
            _Logger.LogDebug("Clear job tables.");

            await using var dbc = _PublishingDbContextFac();
            await using var tx = dbc.BeginTransaction();
            await dbc.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [dbo].[{TableNames.EksEngineInput}]");
            await dbc.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [dbo].[{TableNames.EksEngineOutput}]");
            tx.Commit();
        }

        private async Task Stuff()
        {
            await using var dbc = _PublishingDbContextFac();
            var tekCount = dbc.EksInput.Count(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None);

            if (tekCount == 0)
            {
                _Logger.LogInformation("No stuffing required - No publishable TEKs.");
                return;
            }

            var stuffingCount = tekCount < _EksConfig.TekCountMin ? _EksConfig.TekCountMin - tekCount : 0;
            if (stuffingCount == 0)
            {
                _Logger.LogInformation("No stuffing required - Minimum TEK count OK.");
                return;
            }

            _EksEngineResult.StuffingCount = stuffingCount;

            var stuffing = _EksStuffingGenerator.Execute(new StuffingArgs {Count = stuffingCount, JobTime = _EksEngineResult.Started});

            _Logger.LogInformation("Stuffing required - Count:{Count}.", stuffing.Length);

            await using var tx = dbc.BeginTransaction();
            await dbc.EksInput.AddRangeAsync(stuffing);
            dbc.SaveAndCommit();

            _Logger.LogInformation("Stuffing added.");
        }




        private async Task BuildOutput()
        {
            _Logger.LogDebug("Build EKSs.");
            _BuildEksStopwatch.Start();

            var inputIndex = 0;
            var page = GetInputPage(inputIndex, _EksConfig.PageSize);
            _Logger.LogDebug("Read TEKs - Count:{Count}", page.Length);

            while (page.Length > 0)
            {
                if (_Output.Count + page.Length >= _EksConfig.TekCountMax)
                {
                    _Logger.LogDebug("This page fills the EKS to capacity - Capacity:{Capacity}.", _EksConfig.TekCountMax);
                    var remainingCapacity = _EksConfig.TekCountMax - _Output.Count;
                    AddToOutput(page.Take(remainingCapacity).ToArray()); //Fill to max
                    await WriteNewEksToOutput();
                    AddToOutput(page.Skip(remainingCapacity).ToArray()); //Use leftovers from the page
                }
                else
                {
                    AddToOutput(page);
                }

                inputIndex += _EksConfig.PageSize; //Move input index
                page = GetInputPage(inputIndex, _EksConfig.PageSize); //Read next page.
                _Logger.LogDebug("Read TEKs - Count:{Count}.", page.Length);
            }

            if (_Output.Count > 0)
            {
                _Logger.LogDebug("Write remaining TEKs - Count:{Count}.", _Output.Count);
                await WriteNewEksToOutput();
            }
        }

        private void AddToOutput(EksCreateJobInputEntity[] page)
        {
            _Output.AddRange(page); //Lots of memory
            _Logger.LogDebug("Add TEKs to output - Count:{Count}, Total:{OutputCount}.", page.Length, _Output.Count);
        }

        private static TemporaryExposureKeyArgs Map(EksCreateJobInputEntity c)
            => new TemporaryExposureKeyArgs 
            { 
                RollingPeriod = c.RollingPeriod,
                TransmissionRiskLevel = c.TransmissionRiskLevel,
                KeyData = c.KeyData,
                RollingStartNumber = c.RollingStartNumber
            };

        private async Task WriteNewEksToOutput()
        {
            _Logger.LogDebug("Build EKS.");

            var args = _Output.Select(Map).ToArray();

            var content = await _SetBuilder.BuildAsync(args);
            
            var e = new EksCreateJobOutputEntity
            {
                Region = DefaultValues.Region,
                Release = _EksEngineResult.Started,
                CreatingJobQualifier = ++_EksCount,
                Content = content, 
            };

            _Logger.LogInformation("Write EKS - Id:{CreatingJobQualifier}.", e.CreatingJobQualifier);

            
            await using (var dbc = _PublishingDbContextFac())
            {
                await using var tx = dbc.BeginTransaction();
                await dbc.AddAsync(e);
                dbc.SaveAndCommit();
            }

            _Logger.LogInformation("Mark TEKs as used.");

            foreach (var i in _Output)
                i.Used = true;

            //Could be 750k in this hit
            await using (var dbc2 = _PublishingDbContextFac())
            {
                var bargs = new SubsetBulkArgs
                {
                    PropertiesToInclude = new[] {nameof(EksCreateJobInputEntity.Used)}
                };
                await dbc2.BulkUpdateAsync2(_Output, bargs); //TX
            }

            _EksEngineResult.OutputCount += _Output.Count;

            _EksResults.Add(new EksInfo { TekCount = _Output.Count, TotalSeconds = _BuildEksStopwatch.Elapsed.TotalSeconds });
            _Output.Clear();
        }

        private EksCreateJobInputEntity[] GetInputPage(int skip, int take)
        {
            _Logger.LogDebug("Read page - Skip {Skip}, Take {Take}.", skip, take);

            using var dbc = _PublishingDbContextFac();
            var result = dbc.Set<EksCreateJobInputEntity>()
                .Where(x => x.TransmissionRiskLevel != TransmissionRiskLevel.None)
                .OrderBy(x => x.KeyData).Skip(skip).Take(take).ToArray();

            _Logger.LogDebug("Read page - Count:{Count}.", result.Length);

            return result;
        }

        private async Task CommitResults()
        {
            _Logger.LogInformation("Commit results - publish EKSs.");

            await _ContentWriter.ExecuteAsyc();

            _Logger.LogInformation("Commit results - Mark TEKs as Published.");
            var result = await _MarkWorkFlowTeksAsUsed.ExecuteAsync();
            _Logger.LogInformation("Marked as published - Total:{TotalMarked}.", result.Marked);
            
            await ClearJobTables();
        }
   }
}