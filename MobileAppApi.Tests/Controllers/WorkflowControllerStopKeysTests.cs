﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi;

namespace MobileAppApi.Tests.Controllers
{
    [TestClass]
    public class WorkflowControllerStopKeysTests : WebApplicationFactory<Startup>
    {
        private WebApplicationFactory<Startup> _Factory;

        [TestInitialize]
        public async Task InitializeAsync()
        {
            _Factory = WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                });
            });
        }

        [TestMethod]
        public async Task PostStopKeysTest()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        // Ticket raised to move/refactor, this tests the delay feature not if it's enabled for an endpoint
        /// <summary>
        /// Tests whether or not the response is delayed by at least the minimum time.
        /// </summary>
        /// <param name="min">Minimum wait time</param>
        /// <param name="max">Maximum wait time</param>
        [DataTestMethod]
        [DataRow(100L, 200L)]
        [DataRow(200L, 300L)]
        [DataRow(300L, 400L)]
        [DataRow(50L, 100L)]
        [DataRow(10L, 50L)]
        public async Task DelayTest(long min, long max)
        {
            // Arrange
            var sw = new Stopwatch();
            var client = _Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Workflow:Decoys:DelayInMilliseconds:Min"] = $"{min}",
                        ["Workflow:Decoys:DelayInMilliseconds:Max"] = $"{max}"
                    });
                });
            }).CreateClient();

            // Act
            sw.Start();
            var result = await client.PostAsync("v1/stopkeys", null);
            sw.Stop();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(sw.ElapsedMilliseconds > min);
        }

        // Ticket raised to move/refactor, this tests the delay feature not if it's enabled for an endpoint
        // Ticket raised to implement this as a unit test on ResponsePaddingFilter
        [DataTestMethod]
        [DataRow(8, 16)]
        [DataRow(32, 64)]
        [DataRow(200, 300)]
        public async Task PaddingTest(long min, long max)
        {
            // Arrange
            var client = _Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Workflow:ResponsePadding:ByteCount:Min"] = $"{min}",
                        ["Workflow:ResponsePadding:ByteCount:Max"] = $"{max}",
                        ["Workflow:Decoys:DelayInMilliseconds:Min"] = $"0",
                        ["Workflow:Decoys:DelayInMilliseconds:Max"] = $"8"
                    });
                });
            }).CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);
            var padding = result.Headers.GetValues("padding").SingleOrDefault();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(padding);
            Assert.IsTrue(padding.Length >= min);
            Assert.IsTrue(padding.Length <= max);
        }

        [TestMethod]
        public async Task Stopkeys_has_padding()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/stopkeys", null);

            // Assert
            Assert.IsTrue(result.Headers.Contains("padding"));
        }

        [TestMethod]
        public async Task Register_has_padding()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/register", null);

            // Assert
            Assert.IsTrue(result.Headers.Contains("padding"));
        }

        [TestMethod]
        public async Task Postkeys_has_padding()
        {
            // Arrange
            var client = _Factory.CreateClient();

            // Act
            var result = await client.PostAsync("v1/postkeys", null);

            // Assert
            Assert.IsTrue(result.Headers.Contains("padding"));
        }
    }
}