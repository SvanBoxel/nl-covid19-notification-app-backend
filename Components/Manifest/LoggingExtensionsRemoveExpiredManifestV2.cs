﻿using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifestV2
{
    public static class LoggingExtensionsExpiredManifestV2
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

        public static void WriteStart(this ILogger logger, int keepAliveCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Begin removing expired ManifestV2s - Keep Alive Count:{count}.",
                Name, Start,
                keepAliveCount);
        }

        public static void WriteFinished(this ILogger logger, int zombieCount, int givenMercedesCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finished removing expired ManifestV2s - ExpectedCount:{count} ActualCount:{givenMercy}.",
                Name, Finished,
                zombieCount, givenMercedesCount);
        }

        public static void WriteFinishedNothingRemoved(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finished removing expired ManifestV2s - Nothing to remove.",
                Name, FinishedNothingRemoved);
        }

        public static void WriteRemovingManifests(this ILogger logger, int zombieCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Removing expired ManifestV2s - Count:{count}.",
                Name, RemovingManifests,
                zombieCount);
        }

        public static void WriteRemovingEntry(this ILogger logger, string publishingId, DateTime releaseDate)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Removing expired ManifestV2 - PublishingId:{PublishingId} Release:{Release}.",
                Name, RemovingEntry,
                publishingId, releaseDate);
        }

        public static void WriteReconciliationFailed(this ILogger logger, int reconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] Reconciliation failed removing expired ManifestV2s - Found-GivenMercy-Remaining={reconciliation}.",
                Name, ReconciliationFailed,
                reconciliationCount);
        }

        public static void WriteDeletionReconciliationFailed(this ILogger logger, int deleteReconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] Reconciliation failed removing expired ManifestV2s - Zombies-GivenMercy={deadReconciliation}.",
                Name, DeletionReconciliationFailed,
                deleteReconciliationCount);
        }
    }
}
