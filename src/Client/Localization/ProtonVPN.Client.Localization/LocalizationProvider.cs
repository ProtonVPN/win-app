/*
 * Copyright (c) 2025 Proton AG
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
using System.Reflection;
using Microsoft.Extensions.Localization;
using ProtonVPN.Client.Localization.Building;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ReswPlusLib;
using WinUI3Localizer;

namespace ProtonVPN.Client.Localization;

public class LocalizationProvider : ILocalizationProvider
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly ILocalizer _localizer;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly Lazy<Dictionary<string, string>> _fallbackLanguageDictionary;

    public LocalizationProvider(
        ILogger logger,
        ISettings settings,
        ILocalizerFactory localizerFactory)
    {
        _logger = logger;
        _settings = settings;
        _localizer = localizerFactory.GetOrCreate();
        _stringLocalizer = new StringLocalizer(this);

        _fallbackLanguageDictionary = new Lazy<Dictionary<string, string>>(CreateFallbackLanguageDictionary);

        ForceCurrentLanguageForPluralProvider();
    }

    public void ForceCurrentLanguageForPluralProvider()
    {
        // The library does not allow forcing the language via public interface therefore using reflection.
        try
        {
            Type type = typeof(ResourceLoaderExtension);

            FieldInfo providerField = type.GetField("_pluralProvider", BindingFlags.NonPublic | BindingFlags.Static);
            providerField?.SetValue(null, null);

            MethodInfo method = type.GetMethod("CreatePluralProvider", BindingFlags.NonPublic | BindingFlags.Static);
            method?.Invoke(null, [_settings.Language]);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to set language for plural provider.", e);
        }
    }

    // Only sets the fallback dictionary on Release so that missing translations are easier to detect when on Debug 
    private Dictionary<string, string> CreateFallbackLanguageDictionary()
    {
        return _settings.IsDebugModeEnabled
            ? []
            : _localizer.GetLanguageDictionaries()
                .FirstOrDefault(ld => ld.Language.EqualsIgnoringCase(LocalizerFactory.DEFAULT_LANGUAGE))?
                .GetItems()
                .ToDictionary(i => i.Uid, i => i.Value) ?? new();
    }

    public string Get(string resourceKey)
    {
        string result = _localizer.GetLocalizedString(resourceKey);
        if (result == resourceKey || string.IsNullOrEmpty(result))
        {
            result = _fallbackLanguageDictionary.Value.TryGetValue(resourceKey, out string value)
                ? value
                : resourceKey;
        }
        return result.Replace("\\n", Environment.NewLine);
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

    public string GetPlural(string resourceKey, long number)
    {
        return _stringLocalizer.GetPlural(resourceKey, number);
    }

    public string GetPluralFormat(string resourceKey, long number)
    {
        return string.Format(GetPlural(resourceKey, number), number);
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