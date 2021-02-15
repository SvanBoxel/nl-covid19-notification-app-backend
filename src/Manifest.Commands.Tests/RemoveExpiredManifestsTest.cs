// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
{
    public abstract class RemoveExpiredManifestsTest : IDisposable
    {
        private readonly IDbProvider<ContentDbContext> _ContentDbProvider;
        private Mock<IManifestConfig> _ManifestConfigMock;

        public RemoveExpiredManifestsTest(IDbProvider<ContentDbContext> contentDbProvider)
        {
            _ContentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
        }

        [Theory]
        [InlineData(ContentTypes.Manifest, 1)]
        [InlineData(ContentTypes.ManifestV2, 1)]
        [InlineData(ContentTypes.ManifestV3, 1)]
        [InlineData(ContentTypes.ManifestV4, 1)]
        public void RemoveExpiredManifests_ExecuteForManifestType(string manifestTypeName, int keepAliveCount)
        {
            //Arrange
            _ManifestConfigMock = new Mock<IManifestConfig>();
            _ManifestConfigMock.Setup(x => x.KeepAliveCount).Returns(keepAliveCount);

            CreateManifestsForManifestType(manifestTypeName);

            //Act
            switch (manifestTypeName)
            {
                case ContentTypes.Manifest:
                    var sut = CompileRemoveExpiredManifestsCommand();
                    sut.ExecuteAsync().GetAwaiter().GetResult();
                    break;
                case ContentTypes.ManifestV2:
                    var sutV2 = CompileRemoveExpiredManifestsV2Command();
                    sutV2.ExecuteAsync().GetAwaiter().GetResult();
                    break;
                default:
                    Assert.True(false, $"No {manifestTypeName} remove command exists.");
                    break;
            }

            var result = GetAllManifestsForManifestType(manifestTypeName);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.Count() == keepAliveCount, $"More than {keepAliveCount} {manifestTypeName} remains after deletion.");
        }

        private RemoveExpiredManifestsCommand CompileRemoveExpiredManifestsCommand()
        {
            var logger = new ExpiredManifestLoggingExtensions(new LoggerFactory().CreateLogger<ExpiredManifestLoggingExtensions>());
            var dateTimeProvider = new StandardUtcDateTimeProvider();

            var result = new RemoveExpiredManifestsCommand(_ContentDbProvider.CreateNew, logger, _ManifestConfigMock.Object, dateTimeProvider);

            return result;
        }

        private RemoveExpiredManifestsV2Command CompileRemoveExpiredManifestsV2Command()
        {
            var logger = new ExpiredManifestV2LoggingExtensions(new LoggerFactory().CreateLogger<ExpiredManifestV2LoggingExtensions>());
            var dateTimeProvider = new StandardUtcDateTimeProvider();

            var result = new RemoveExpiredManifestsV2Command(_ContentDbProvider.CreateNew, logger, _ManifestConfigMock.Object, dateTimeProvider);

            return result;
        }

        private void CreateManifestsForManifestType(string manifestTypeName)
        {
            var database = _ContentDbProvider.CreateNew();

            var content = "This is a Manifest";

            var today = DateTime.UtcNow;
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

            database.Content.AddRange(new[]
            {
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(content),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = twoDaysAgo,
                    Release = twoDaysAgo

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(content),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = yesterday,
                    Release = yesterday

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(content),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = today,
                    Release = today

                },
            });

            database.SaveChanges();
        }

        private IEnumerable<ContentEntity> GetAllManifestsForManifestType(string manifestTypeName)
        {
            var database = _ContentDbProvider.CreateNew();
            return database.Set<ContentEntity>().Where(x => x.Type == manifestTypeName);
        }

        public void Dispose()
        {
            _ContentDbProvider.Dispose();
        }
    }
}