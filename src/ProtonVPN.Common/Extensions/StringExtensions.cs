/*
 * Copyright (c) 2023 Proton AG
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ProtonVPN.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex Base64KeyRegex = new("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$");

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool EqualsIgnoringCase(this string value, string other)
        {
            return value.Equals(other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsIgnoringCase(this string value, string other)
        {
            return value != null && value.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool ContainsIgnoringCase(this IEnumerable<string> collection, string other)
        {
            return collection != null && collection.Any(e => e.Equals(other, StringComparison.OrdinalIgnoreCase));
        }

        public static bool StartsWithIgnoringCase(this string value, string other)
        {
            return value != null && value.StartsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithIgnoringCase(this string value, string other)
        {
            return value != null && value.EndsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNotNullAndContains(this string value, string other)
        {
            return value != null && value.Contains(other);
        }

        public static string FirstCharToUpper(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.First().ToString().ToUpper() + value.Substring(1);
        }

        public static string TrimEnd(this string value, string ending)
        {
            if (!value.EndsWith(ending))
            {
                return value;
            }

            return value.Remove(value.LastIndexOf(ending));
        }

        public static string GetLastChars(this string value, int length)
        {
            return length >= value.Length ? value : value.Substring(value.Length - length);
        }

        public static string GetFirstChars(this string value, int maxLength)
        {
            return value?.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        public static byte[] HexStringToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static bool IsValidIpAddress(this string ip)
        {
            return IPAddress.TryParse(ip, out var parsedIpAddress) &&
                   ip.EqualsIgnoringCase(parsedIpAddress.ToString());
        }

        public static bool IsValidBase64Key(this string key)
        {
            return Base64KeyRegex.IsMatch(key);
        }

        public static uint ToIPAddressBytes(this string value)
        {
            if (IPAddress.TryParse(value, out IPAddress address))
            {
                return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            }

            return 0;
        }
    }
}