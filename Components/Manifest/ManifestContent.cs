﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class ManifestContent
    {
        public string[] ExposureKeySets { get; set; }

        public string RiskCalculationParameters { get; set; }

        public string AppConfig { get; set; }


        protected bool Equals(ManifestContent other)
        {
            return ExposureKeySets.SequenceEqual(other.ExposureKeySets) && RiskCalculationParameters == other.RiskCalculationParameters && AppConfig == other.AppConfig;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ManifestContent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ExposureKeySets.GetHashCode();
                hashCode = (hashCode * 397) ^ RiskCalculationParameters.GetHashCode();
                hashCode = (hashCode * 397) ^ AppConfig.GetHashCode();
                return hashCode;
            }
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}