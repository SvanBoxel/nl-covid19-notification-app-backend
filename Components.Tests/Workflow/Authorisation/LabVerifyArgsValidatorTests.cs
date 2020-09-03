// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Auth;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.Authorisation
{
    public class LabVerifyArgsValidatorTests
    {
        private LabVerifyArgsValidator _Validator;
        private IPollTokenService _PollTokenService;
        private const double ClaimLifetimeHours = 3;
        private IUtcDateTimeProvider _UtcDateTimeProvider;

        public LabVerifyArgsValidatorTests()
        {
            var lf = new TestLogger<JwtService>();
            _UtcDateTimeProvider = new StandardUtcDateTimeProvider();
            _PollTokenService =
                new PollTokenService(
                    new JwtService(
                        new HardCodedIccPortalConfig(null, "http://test.test", "test_secret123", ClaimLifetimeHours, true),
                        _UtcDateTimeProvider, lf), _UtcDateTimeProvider);
            _Validator = new LabVerifyArgsValidator(_PollTokenService);
        }

        [Fact]
        public void Validate_checks_null()
        {
            // Assemble 
            var args = new LabVerifyArgs();
            // Act
            var result = _Validator.Validate(args);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public void Validate_checks_pollToken_format_1()
        {
            // Assemble 
            var args = new LabVerifyArgs {PollToken = "test_not_base64"};
            // Act
            var result = _Validator.Validate(args);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_checks_pollToken_format_2()
        {
            // Assemble 
            var args = new LabVerifyArgs {PollToken = "test_not_base64.part2.part3"};
            // Act
            var result = _Validator.Validate(args);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_checks_pollToken_valid()
        {
            // Assemble 
            var token = _PollTokenService.Next();
            var args = new LabVerifyArgs {PollToken = token};
            // Act
            var result = _Validator.Validate(args);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_checks_pollToken_expires()
        {
            // Assemble 
            var token = _PollTokenService.Next();
            var args = new LabVerifyArgs() { PollToken = token };

            throw new System.Exception("Finish this test.");
            // Act
            // var mock = new Mock<IDateTime>();
            // mock.Setup(fake => fake.Now)
            // .Returns(new DateTime(2000, 1, 1));
            var result = _Validator.Validate(args);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_checks_pollToken_signature()
        {
            // Assemble 
            var token = _PollTokenService.Next();

            token = token.Substring(0, token.Length - 2); // modifies end of token which is signature

            var args = new LabVerifyArgs {PollToken = token};

            // Act
            var result = _Validator.Validate(args);

            // Assert
            Assert.False(result);
        }
    }
}