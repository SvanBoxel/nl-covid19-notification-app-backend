﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stats
{
    /// TODO use sqlite/sqlserver providers
    [ExclusivelyUses(nameof(StatsTests))]
    public class StatsTests
    {
        private Func<WorkflowDbContext> _WorkflowFac;
        private Func<StatsDbContext> _StatsFac;
        private IUtcDateTimeProvider _FakeDtp;

        public StatsTests()
        {
            _WorkflowFac = () => new WorkflowDbContext(new DbContextOptionsBuilder()
                .UseSqlServer("Initial Catalog=WorkflowStatTest1; Server=.;Persist Security Info=True;Integrated Security=True;Connection Timeout=60;")
                .Options);

            var wf = _WorkflowFac();
            wf.Database.EnsureDeleted();
            wf.Database.EnsureCreated();

            _StatsFac = () => new StatsDbContext(new DbContextOptionsBuilder()
                .UseSqlServer("Initial Catalog=StatsTest1; Server=.;Persist Security Info=True;Integrated Security=True;Connection Timeout=60;")
                .Options);

            var stats = _StatsFac();
            stats.Database.EnsureDeleted();
            stats.Database.EnsureCreated();

            //_FakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };
            //_Lf = new LoggerFactory();

            var snapshot = DateTime.UtcNow;
            var mock = new Mock<IUtcDateTimeProvider>(MockBehavior.Strict);
            mock.Setup(x => x.Snapshot).Returns(snapshot);
            _FakeDtp = mock.Object;
        }

        [Fact]
        public void NothingToSeeHere()
        {
            var wf = _WorkflowFac();
            var statsDbContext = _StatsFac();
            var cmd = new StatisticsCommand(new StatisticsDbWriter(statsDbContext, _FakeDtp),
                new IStatsQueryCommand[]
                {
                    new TotalWorkflowCountStatsQueryCommand(wf),
                    new TotalWorkflowsWithTeksQueryCommand(wf),
                    new TotalWorkflowAuthorisedCountStatsQueryCommand(wf),
                    new PublishedTekCountStatsQueryCommand(wf),
                    new TotalTekCountStatsQueryCommand(wf),
                });
                cmd.Execute();

                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowCountStatsQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowsWithTeksQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowAuthorisedCountStatsQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == PublishedTekCountStatsQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalTekCountStatsQueryCommand.Name).Value);
        }

        [Fact]
        public void WithData()
        {
            var wf = _WorkflowFac();

            var workflows = new[] {
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{1}, ConfirmationKey = new byte[]{1}, LabConfirmationId = "1", Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>()},
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{2}, ConfirmationKey = new byte[]{2}, LabConfirmationId = "2",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{3}, ConfirmationKey = new byte[]{3}, LabConfirmationId = "3",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{4}, ConfirmationKey = new byte[]{4}, LabConfirmationId = "4",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{5}, ConfirmationKey = new byte[]{5}, LabConfirmationId = "5",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{6}, ConfirmationKey = new byte[]{6}, LabConfirmationId = null,  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{7}, ConfirmationKey = new byte[]{7}, LabConfirmationId = null,  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{8}, ConfirmationKey = new byte[]{8}, LabConfirmationId = null,  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() }
            };

            ((List<TekEntity>) workflows[0].Teks).AddRange(new []{new TekEntity{KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue } });


            ((List<TekEntity>)workflows[4].Teks).AddRange(new[]
            { new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue },
            });

            ((List<TekEntity>)workflows[5].Teks).AddRange(new[]
            { new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue }
            });

            wf.KeyReleaseWorkflowStates.AddRange(workflows);
            wf.SaveChanges();

            var statsDbContext = _StatsFac();
            var cmd = new StatisticsCommand(new StatisticsDbWriter(statsDbContext, _FakeDtp),
                new IStatsQueryCommand[]
                {
                    new TotalWorkflowCountStatsQueryCommand(wf),
                    new TotalWorkflowsWithTeksQueryCommand(wf),
                    new TotalWorkflowAuthorisedCountStatsQueryCommand(wf),
                    new PublishedTekCountStatsQueryCommand(wf),
                    new TotalTekCountStatsQueryCommand(wf),
                });
            cmd.Execute();

            Assert.Equal(8, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowCountStatsQueryCommand.Name).Value);
            Assert.Equal(3, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowsWithTeksQueryCommand.Name).Value);
            Assert.Equal(3, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowAuthorisedCountStatsQueryCommand.Name).Value);
            Assert.Equal(4, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == PublishedTekCountStatsQueryCommand.Name).Value);
            Assert.Equal(9, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalTekCountStatsQueryCommand.Name).Value);
        }
    }
}
