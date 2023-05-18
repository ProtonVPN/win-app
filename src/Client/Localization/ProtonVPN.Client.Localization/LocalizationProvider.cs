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
using ProtonVPN.Client.Localization.Contracts;
using WinUI3Localizer;

namespace ProtonVPN.Client.Localization;

public class LocalizationProvider : ILocalizationProvider
{
    private readonly ILocalizer _localizer = Localizer.Get();

    public string Get(string resourceKey)
    {
        return _localizer.GetLocalizedString(resourceKey)
            .Replace("\\n", Environment.NewLine);
    }

    public string GetFormat(string resourceKey, object arg0)
    {
        return Safe(resourceKey, value => string.Format(value, arg0));
    }

    public string GetFormat(string resourceKey, object arg0, object arg1)
    {
        return Safe(resourceKey, value => string.Format(value, arg0, arg1));
    }

    public string GetFormat(string resourceKey, object arg0, object arg1, object arg2)
    {
        return Safe(resourceKey, value => string.Format(value, arg0, arg1, arg2));
    }

    public string GetFormat(string resourceKey, params object[] args)
    {
        return Safe(resourceKey, value => string.Format(value, args));
    }

    private string Safe(string resourceKey, Func<string, string> func)
    {
        string value = string.Empty;

        try
        {
            value = Get(resourceKey);

            return func(value);
        }
        catch (FormatException)
        {
            return value;
        }
    }
}
