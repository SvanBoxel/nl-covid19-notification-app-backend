﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Stuffing
{
    public class WriteStuffingToDiagnosisKeys : IWriteStuffingToDiagnosisKeys
    {
        private readonly DkSourceDbContext _DkDbContext;
        private readonly EksPublishingJobDbContext _EksPublishingDbContext;
        private readonly IDiagnosticKeyProcessor[] _DkProcessors;
        public WriteStuffingToDiagnosisKeys(DkSourceDbContext dkDbContext, EksPublishingJobDbContext eksPublishingDbContext, IDiagnosticKeyProcessor[] dkProcessors)
        {
            _DkDbContext = dkDbContext ?? throw new ArgumentNullException(nameof(dkDbContext));
            _EksPublishingDbContext = eksPublishingDbContext ?? throw new ArgumentNullException(nameof(eksPublishingDbContext));
            _DkProcessors = dkProcessors ?? throw new ArgumentNullException(nameof(eksPublishingDbContext));
        }

        public async Task ExecuteAsync()
        {
            var stuffing = _EksPublishingDbContext.Set<EksCreateJobInputEntity>()
                .Where(x => x.TekId == null && x.Used)
                .OrderBy(x => x.TekId)
                .Select(x => 
                    new DiagnosisKeyEntity
                    { 
                        PublishedLocally = true,
                        DailyKey = new DailyKey(x.KeyData, x.RollingStartNumber, x.RollingPeriod),
                        Local = new LocalTekInfo 
                        { 
                            TransmissionRiskLevel = x.TransmissionRiskLevel,
                            //TODO We have to fake THIS, not the TRL - build table of delta compared to RSN and create fake DateOfSymptomsOnset not TRL, then DERIVE TRL
                            DaysSinceSymptomsOnset = x.DaysSinceSymptomsOnset, 
                        }
                    })
                .ToArray();

            var items = stuffing
                .Select(x => (DkProcessingItem?) new DkProcessingItem
                {
                    DiagnosisKey = x,
                    Metadata = new Dictionary<string, object>()
                }).ToArray();

            items = _DkProcessors.Execute(items);
            var results = items.Select(x => x.DiagnosisKey).ToList(); //Can't get rid of compiler warning.
            await _DkDbContext.BulkInsertAsync2(results, new SubsetBulkArgs()); //TX
        }
    }
}