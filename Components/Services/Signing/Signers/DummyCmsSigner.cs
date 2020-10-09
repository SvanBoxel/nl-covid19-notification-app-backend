﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
	/// <summary>
	/// Returns an byte array that indicates no RSA-signature is created
	/// </summary>
	public class DummyCmsSigner : IContentSigner
	{
		public string SignatureOid => "2.16.840.1.101.3.4.2.1"; //Unused?

		public byte[] DummyContent => Encoding.ASCII.GetBytes("Signature intentionally left empty");

		public byte[] GetSignature(byte[] content)
		{
			return DummyContent;
		}

	}
}
