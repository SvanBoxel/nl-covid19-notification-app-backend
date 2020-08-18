// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "jwt";

        //private const string AccessTokenElement = "access_token";

        private readonly IJwtService _JwtService;
        private readonly ITheIdentityHubService _TheIdentityHubService;

        public JwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            IJwtService jwtService,
            ITheIdentityHubService theIdentityHubService) : base(options, loggerFactory, encoder, clock)
        {
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _TheIdentityHubService =
                theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                Logger.LogInformation("Missing authorization header.");
                return AuthenticateResult.Fail("Missing authorization header.");
            }

            if (!AuthenticationHeaderValue.TryParse(headerValue, out var authHeader))
            {
                Logger.LogInformation("Invalid authorization header.");
                return AuthenticateResult.Fail("Invalid authorization header.");
            }

            var jwt = authHeader.ToString().Replace("Bearer ", "").Trim();

            if (!_JwtService.TryDecode(jwt, out var decodedClaims))
            {
                return AuthenticateResult.Fail("Invalid jwt token");
            }

            if (!await _TheIdentityHubService.VerifyToken(decodedClaims["access_token"]))
            {
                return AuthenticateResult.Fail("Invalid jwt token");
            }


            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, decodedClaims["id"]),
                new Claim(TheIdentityHubClaimTypes.AccessToken, decodedClaims["access_token"]),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}