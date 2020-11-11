﻿using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifestV2
{
    public class ExpiredManifestV2LoggingExtensions
    {
        private const string Name = "RemoveExpiredManifestV2";
        private const int Base = LoggingCodex.RemoveExpiredManifestV2;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int RemovingManifests = Base + 1;
        private const int RemovingEntry = Base + 2;
        private const int ReconciliationFailed = Base + 3;
        private const int DeletionReconciliationFailed = Base + 4;
        private const int FinishedNothingRemoved = Base + 98;

        private readonly ILogger _Logger;

        public ExpiredManifestV2LoggingExtensions(ILogger<ExpiredManifestV2LoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart(int keepAliveCount)
        {
            _Logger.LogInformation("[{name}/{id}] Begin removing expired ManifestV2s - Keep Alive Count:{count}.",
                Name, Start,
                keepAliveCount);
        }

        public void WriteFinished(int zombieCount, int givenMercedesCount)
        {
            _Logger.LogInformation("[{name}/{id}] Finished removing expired ManifestV2s - ExpectedCount:{count} ActualCount:{givenMercy}.",
                Name, Finished,
                zombieCount, givenMercedesCount);
        }

        public void WriteFinishedNothingRemoved()
        {
            _Logger.LogInformation("[{name}/{id}] Finished removing expired ManifestV2s - Nothing to remove.",
                Name, FinishedNothingRemoved);
        }

        public void WriteRemovingManifests(int zombieCount)
        {
            _Logger.LogInformation("[{name}/{id}] Removing expired ManifestV2s - Count:{count}.",
                Name, RemovingManifests,
                zombieCount);
        }

        public void WriteRemovingEntry(string publishingId, DateTime releaseDate)
        {
            _Logger.LogInformation("[{name}/{id}] Removing expired ManifestV2 - PublishingId:{PublishingId} Release:{Release}.",
                Name, RemovingEntry,
                publishingId, releaseDate);
        }

        public void WriteReconciliationFailed(int reconciliationCount)
        {
            _Logger.LogError("[{name}/{id}] Reconciliation failed removing expired ManifestV2s - Found-GivenMercy-Remaining={reconciliation}.",
                Name, ReconciliationFailed,
                reconciliationCount);
        }

        public void WriteDeletionReconciliationFailed(int deleteReconciliationCount)
        {
            _Logger.LogError("[{name}/{id}] Reconciliation failed removing expired ManifestV2s - Zombies-GivenMercy={deadReconciliation}.",
                Name, DeletionReconciliationFailed,
                deleteReconciliationCount);
        }
    }
}
