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

using System.Globalization;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Common.Core.Extensions;

public static class StringExtensions
{
    private static readonly Regex _base64KeyRegex = new("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$");

    private static readonly Dictionary<string, string> _diacriticsMap = new()
    {
        { "äæǽ", "ae" },
        { "öœ", "oe" },
        { "ü", "ue" },
        { "Ä", "Ae" },
        { "Ü", "Ue" },
        { "Ö", "Oe" },
        { "ÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶА", "A" },
        { "àáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặа", "a" },
        { "Б", "B" },
        { "б", "b" },
        { "ÇĆĈĊČ", "C" },
        { "çćĉċč", "c" },
        { "Д", "D" },
        { "д", "d" },
        { "ÐĎĐΔ", "Dj" },
        { "ðďđδ", "dj" },
        { "ÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭ", "E" },
        { "èéêëēĕėęěέεẽẻẹềếễểệеэ", "e" },
        { "Ф", "F" },
        { "ф", "f" },
        { "ĜĞĠĢΓГҐ", "G" },
        { "ĝğġģγгґ", "g" },
        { "ĤĦ", "H" },
        { "ĥħ", "h" },
        { "ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫ", "I" },
        { "ìíîïĩīĭǐįıηήίιϊỉịиыї", "i" },
        { "Ĵ", "J" },
        { "ĵ", "j" },
        { "ĶΚК", "K" },
        { "ķκк", "k" },
        { "ĹĻĽĿŁΛЛ", "L" },
        { "ĺļľŀłλл", "l" },
        { "М", "M" },
        { "м", "m" },
        { "ÑŃŅŇΝН", "N" },
        { "ñńņňŉνн", "n" },
        { "ÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢО", "O" },
        { "òóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợо", "o" },
        { "П", "P" },
        { "п", "p" },
        { "ŔŖŘΡР", "R" },
        { "ŕŗřρр", "r" },
        { "ŚŜŞȘŠΣС", "S" },
        { "śŝşșšſσςс", "s" },
        { "ȚŢŤŦτТ", "T" },
        { "țţťŧт", "t" },
        { "ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУ", "U" },
        { "ùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựу", "u" },
        { "ÝŸŶΥΎΫỲỸỶỴЙ", "Y" },
        { "ýÿŷỳỹỷỵй", "y" },
        { "В", "V" },
        { "в", "v" },
        { "Ŵ", "W" },
        { "ŵ", "w" },
        { "ŹŻŽΖЗ", "Z" },
        { "źżžζз", "z" },
        { "ÆǼ", "AE" },
        { "ß", "ss" },
        { "Ĳ", "IJ" },
        { "ĳ", "ij" },
        { "Œ", "OE" },
        { "ƒ", "f" },
        { "ξ", "ks" },
        { "π", "p" },
        { "β", "v" },
        { "μ", "m" },
        { "ψ", "ps" },
        { "Ё", "Yo" },
        { "ё", "yo" },
        { "Є", "Ye" },
        { "є", "ye" },
        { "Ї", "Yi" },
        { "Ж", "Zh" },
        { "ж", "zh" },
        { "Х", "Kh" },
        { "х", "kh" },
        { "Ц", "Ts" },
        { "ц", "ts" },
        { "Ч", "Ch" },
        { "ч", "ch" },
        { "Ш", "Sh" },
        { "ш", "sh" },
        { "Щ", "Shch" },
        { "щ", "shch" },
        { "ЪъЬь", "" },
        { "Ю", "Yu" },
        { "ю", "yu" },
        { "Я", "Ya" },
        { "я", "ya" },
    };

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool? ToBoolOrNull(this string value)
    {
        return bool.TryParse(value, out bool result) ? result : null;
    }

    public static bool EqualsIgnoringCase(this string value, string other)
    {
        return value.Equals(other, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsIgnoringCase(this string value, string other)
    {
        return value != null && value.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string RemoveDiacritics(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        value = value.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new();

        foreach (char c in value.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
        {
            // Some of the diacritics can't be automatically remapped, therefore requires manual mapping.
            bool isMapped = false;
            foreach (KeyValuePair<string, string> kvp in _diacriticsMap.Where(kvp => kvp.Key.Contains(c)))
            {
                sb.Append(kvp.Value);
                isMapped = true;
                break;
            }

            if (!isMapped)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
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

    public static string? GetFirstChars(this string? value, int maxLength)
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

    public static bool IsValidIpAddressFormat(this string ip)
    {
        return IPAddress.TryParse(ip, out IPAddress? parsedIpAddress) &&
               ip.EqualsIgnoringCase(parsedIpAddress.ToString());
    }

    public static bool IsValidIpAddress(this string ip)
    {
        return NetworkAddress.TryParse(ip, out NetworkAddress address)
            && address.IsSingleIp;
    }

    public static bool IsValidIpAddressOrRange(this string ip)
    {
        return NetworkAddress.TryParse(ip, out _);
    }

    public static bool IsValidBase64Key(this string key)
    {
        return _base64KeyRegex.IsMatch(key);
    }

    public static bool IsValidUrl(this string url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && Uri.IsWellFormedUriString(url.ToFormattedUrl(), UriKind.Absolute);
    }

    public static string ToFormattedUrl(this string url)
    {
        // Check if protocol is already defined, otherwise use https by default.
        return string.IsNullOrWhiteSpace(url) || url.Contains(":/")
            ? url
            : $"https://{url}";
    }

    public static uint ToIPAddressBytes(this string value)
    {
        if (IPAddress.TryParse(value, out IPAddress? address))
        {
            return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
        }

        return 0;
    }

    public static string FormatIfNotEmpty(this string value, string format)
    {
        if (value.IsNullOrEmpty())
        {
            return value;
        }

        return string.Format(format, value);
    }

    public static IEnumerable<string>? SplitToEnumerable(this string value, char separator)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Split(separator).Select(e => e.Trim());
    }

    public static HashSet<string>? SplitToHashSet(this string value, char separator)
    {
        return value.SplitToEnumerable(separator)?.ToHashSet();
    }

    public static List<string>? SplitToList(this string value, char separator)
    {
        return value.SplitToEnumerable(separator)?.ToList();
    }

    public static bool IsHttpUri(this string uriString, out Uri? uri)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            || Uri.TryCreate($"https://{uriString}", UriKind.Absolute, out uri);
    }

    public static bool IsValidEmailAddress(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            string DomainMapper(Match match)
            {
                IdnMapping idn = new();
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static SecureString ToSecureString(this string? value)
    {
        SecureString secureString = new();

        if (!string.IsNullOrEmpty(value))
        {
            foreach (char c in value)
            {
                secureString.AppendChar(c);
            }
        }

        secureString.MakeReadOnly();

        return secureString;
    }

    public static string NormalizeCountryCode(this string countryCode)
    {
        if (string.Equals(countryCode, "GB", StringComparison.OrdinalIgnoreCase))
        {
            // Normalize GB to UK for consistency
            return "UK";
        }

        return countryCode.Trim().ToUpperInvariant();
    }
}