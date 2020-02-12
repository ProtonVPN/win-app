/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ProtonVPN.Core.Api.Handlers.TlsPinning
{
    internal class PublicKeyInfoHash
    {
        private readonly X509Certificate _certificate;

        public PublicKeyInfoHash(X509Certificate certificate)
        {
            _certificate = certificate;
        }

        public string Value()
        {
            var subjectPublicKeyInfo = GetSubjectPublicKeyInfoRaw(_certificate);
            byte[] digest;
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                digest = sha256.ComputeHash(subjectPublicKeyInfo);
            }

            return Convert.ToBase64String(digest);
        }

        private byte[] GetSubjectPublicKeyInfoRaw(X509Certificate x509Cert)
        {
            var rawCert = x509Cert.GetRawCertData();
            var list = AsnNext(ref rawCert, true);
            var tbsCertificate = AsnNext(ref list, false);
            list = AsnNext(ref tbsCertificate, true);

            AsnNext(ref list, false);
            AsnNext(ref list, false);
            AsnNext(ref list, false);
            AsnNext(ref list, false);
            AsnNext(ref list, false);
            AsnNext(ref list, false);

            return AsnNext(ref list, false);
        }

        private byte[] AsnNext(ref byte[] buffer, bool unwrap)
        {
            byte[] result;

            if (buffer.Length < 2)
            {
                result = buffer;
                buffer = new byte[0];
                return result;
            }

            var index = 0;
            index += 1;

            int length = buffer[index];
            index += 1;

            var lengthBytes = 1;
            if (length >= 0x80)
            {
                lengthBytes = length & 0x0F;
                length = 0;

                for (var i = 0; i < lengthBytes; i++)
                {
                    length = (length << 8) + buffer[2 + i];
                    index += 1;
                }
                lengthBytes++;
            }

            int copyStart;
            int copyLength;
            if (unwrap)
            {
                copyStart = 1 + lengthBytes;
                copyLength = length;
            }
            else
            {
                copyStart = 0;
                copyLength = 1 + lengthBytes + length;
            }
            result = new byte[copyLength];
            Array.Copy(buffer, copyStart, result, 0, copyLength);

            var remaining = new byte[buffer.Length - (copyStart + copyLength)];
            if (remaining.Length > 0)
            {
                Array.Copy(buffer, copyStart + copyLength, remaining, 0, remaining.Length);
            }
            buffer = remaining;

            return result;
        }
    }
}
