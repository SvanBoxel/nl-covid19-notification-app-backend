﻿using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi.Services
{
    public interface IPublishTekService
    {
        Task<PublishTekResponse> ExecuteAsync(PublishTekArgs args);
    }
}
