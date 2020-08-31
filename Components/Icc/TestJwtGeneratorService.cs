// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class TestJwtGeneratorService
    {
        private readonly IJwtService _JwtService;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger<TestJwtGeneratorService> _Logger;
        
        public TestJwtGeneratorService(IJwtService jwtService, IUtcDateTimeProvider dateTimeProvider,
            ILogger<TestJwtGeneratorService> logger)
        {
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        
            _Logger.LogInformation("TestJwtGeneratorService Singleton constructed, generating test JWT now...");
            var testJwtData = new Dictionary<string, object> {{"access_token", "test_access_token"}, {"id", "0"}};
        
            var expiry = new StandardUtcDateTimeProvider().Now().AddDays(7).ToUnixTimeU64();
        
            _Logger.LogInformation(jwtService.Generate(expiry, testJwtData));
        }
    }
}