﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class TestHttpResponseHeaderConfig : IHttpResponseHeaderConfig
    {
        public TestHttpResponseHeaderConfig(int maxTtl = 1209600)
        {
            EksMaxTtl = maxTtl;
        }

        public string ManifestCacheControl => string.Empty;
        public int EksMaxTtl { get; }
    }
}