﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace ForceTekAuth
{
    internal class ForceTekAuthCommand
    {
        private readonly WorkflowDbContext _WorkflowDb;
        private readonly IUtcDateTimeProvider _Dtp;

        public ForceTekAuthCommand(WorkflowDbContext workflowDb, IUtcDateTimeProvider dtp)
        {
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _Dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
        }

        public void Execute(string[] _)
        {
            using var tx = _WorkflowDb.BeginTransaction();

            ForceWorkflowAuth();
            //ForceTekAuth();

            _WorkflowDb.SaveAndCommit();
        }

        private void ForceWorkflowAuth()
        {
            var notAuthed = _WorkflowDb.KeyReleaseWorkflowStates
                .Where(x => x.AuthorisedByCaregiver == null)
                .ToList();

            Console.WriteLine($"Found: {notAuthed.Count}.");

            foreach (var i in notAuthed)
            {
                i.LabConfirmationId = null;
                i.AuthorisedByCaregiver = _Dtp.Snapshot;
                i.DateOfSymptomsOnset = _Dtp.Snapshot.Date.AddDays(-1);
            }

            _WorkflowDb.BulkUpdate(notAuthed);
        }

        //private void ForceTekAuth()
        //{
        //    var teks = _WorkflowDb.TemporaryExposureKeys
        //        .Include(x => x.Owner)
        //        .Where(x => x.PublishingState == PublishingState.Unpublished) // && x.PublishAfter > _Dtp.Snapshot
        //        .ToList();

        //    foreach (var i in teks)
        //    {
        //        i.PublishAfter = _Dtp.Snapshot;
        //        _WorkflowDb.TemporaryExposureKeys.Update(i);
        //    }
        //}
    }
}