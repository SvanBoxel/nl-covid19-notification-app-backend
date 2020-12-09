﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing.Providers
{
    /// <summary>
    /// Embedded resources or file system.
    /// </summary>
    public interface ICertificateLocationConfig
    {
        public string Path { get; }
        public string Password { get; }
    }
}