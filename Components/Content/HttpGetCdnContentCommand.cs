﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Includes mitigations for CDN cache miss/stale item edge cases.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetCdnContentCommand
    {
        private readonly ContentDbContext _DbContext;
        private readonly IPublishingIdService _PublishingIdService;
        private readonly ILogger _Logger;
        private readonly IHttpResponseHeaderConfig _HttpResponseHeaderConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public HttpGetCdnContentCommand(ContentDbContext dbContext, IPublishingIdService publishingIdService, ILogger<HttpGetCdnContentCommand> logger, IHttpResponseHeaderConfig httpResponseHeaderConfig, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _PublishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _HttpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        /// <summary>
        /// Immutable content
        /// </summary>
        public async Task Execute(HttpContext httpContext, string type, string id)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (!ContentTypes.IsValid(type))
            {
                _Logger.LogError("Invalid generic content type - {Id}.", id);
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            if (!_PublishingIdService.Validate(id))
            {
                _Logger.LogError("Invalid content id - {Id}.", id);
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            if (!httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                _Logger.LogError("Required request header missing - if-none-match.");
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400;
            }

            var content = await _DbContext.SafeGetContent(type, id, _DateTimeProvider.Snapshot);
            
            if (content == null)
            {
                _Logger.LogError("Content not found - {Id}.", id);
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return;
            }

            if (etagValue == content.PublishingId)
            {
                _Logger.LogWarning("Matching etag found, responding with 304 - {Id}.", id);
                httpContext.Response.StatusCode = 304;
                httpContext.Response.ContentLength = 0;
                return;
            }

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", content.ContentTypeName);
            httpContext.Response.Headers.Add("cache-control", _HttpResponseHeaderConfig.ImmutableContentCacheControl);
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = content.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            await httpContext.Response.Body.WriteAsync(content.Content);
        }
    }
}