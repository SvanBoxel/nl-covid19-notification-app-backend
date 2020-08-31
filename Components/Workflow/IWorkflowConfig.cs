﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public interface IWorkflowConfig
    {
        public int TimeToLiveMinutes { get; }
        public int PermittedMobileDeviceClockErrorMinutes { get; }
        int PostKeysSignatureLength { get; }
        int BucketIdLength { get; }
        int ConfirmationKeyLength { get; }
        bool CleanupDeletesData { get; }
    }
}