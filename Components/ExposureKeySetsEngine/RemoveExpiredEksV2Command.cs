﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredEksV2;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class RemoveExpiredEksV2Command
    {
        private readonly ContentDbContext _DbContext;
        private readonly IEksConfig _Config;
        private readonly IUtcDateTimeProvider _Dtp;
        private readonly ExpiredEksV2LoggingExtensions _Logger;

        public RemoveExpiredEksV2Command(ContentDbContext dbContext, IEksConfig config, IUtcDateTimeProvider dtp, ExpiredEksV2LoggingExtensions logger)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RemoveExpiredEksCommandResult Execute()
        {
            var result = new RemoveExpiredEksCommandResult();

            _Logger.WriteStart();

            var cutoff = (_Dtp.Snapshot - TimeSpan.FromDays(_Config.LifetimeDays)).Date;

            using (var tx = _DbContext.BeginTransaction())
            {
                result.Found = _DbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2);
                _Logger.WriteCurrentEksFound(result.Found);

                var zombies = _DbContext.Content
                    .Where(x => x.Type == ContentTypes.ExposureKeySetV2 && x.Release < cutoff)
                    .Select(x => new { x.PublishingId, x.Release })
                    .ToList();

                result.Zombies = zombies.Count;

                _Logger.WriteTotalEksFound(cutoff, result.Zombies);
                foreach (var i in zombies)
                    _Logger.WriteEntryFound(i.PublishingId, i.Release);

                if (!_Config.CleanupDeletesData)
                {
                    _Logger.WriteFinishedNothingRemoved();
                    result.Remaining = result.Found;
                    return result;
                }

                result.GivenMercy = _DbContext.Database.ExecuteSqlInterpolated($"DELETE FROM [Content] WHERE [Type] = {ContentTypes.ExposureKeySetV2} AND [Release] < {cutoff}");
                tx.Commit();
                //Implicit tx
                result.Remaining = _DbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2);
            }

            _Logger.WriteRemovedAmount(result.GivenMercy, result.Remaining);

            if (result.Reconciliation != 0)
                _Logger.WriteReconciliationFailed(result.Reconciliation);

            _Logger.WriteFinished();
            return result;
        }
    }
}