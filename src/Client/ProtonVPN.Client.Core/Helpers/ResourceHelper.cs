/*
 * Copyright (c) 2024 Proton AG
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

using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using Windows.UI;
using System.Linq;
using System.IO;

namespace ProtonVPN.Client.Core.Helpers;

public static class ResourceHelper
{
    private const string COLORS_DICTIONARY = "Styles/Colors.xaml";
    private const string ILLUSTRATIONS_DICTIONARY = "Styles/Illustrations.xaml";
    private const string ICONS_DICTIONARY = "Styles/Icons.xaml";
    private const string FLYOUT_STYLES_DICTIONARY = "Styles/Controls/FlyoutStyles.xaml";

    public static Color GetColor(string resourceKey, ElementTheme theme)
    {
        string dictionaryName = COLORS_DICTIONARY;

        return TryGetResource<Color?>(dictionaryName, resourceKey, theme)
            ?? TryGetResource<Color?>(dictionaryName, resourceKey)
            ?? throw new ArgumentException($"Resource '{resourceKey}' not found in {dictionaryName}");
    }

    public static ImageSource GetIllustration(string resourceKey, ElementTheme theme)
    {
        string dictionaryName = ILLUSTRATIONS_DICTIONARY;

        return TryGetResource<ImageSource>(dictionaryName, resourceKey, theme)
            ?? TryGetResource<ImageSource>(dictionaryName, resourceKey)
            ?? throw new ArgumentException($"Resource '{resourceKey}' not found in {dictionaryName}");
    }

    public static ImageSource GetIllustration(string resourceKey)
    {
        string dictionaryName = ILLUSTRATIONS_DICTIONARY;

        return TryGetResource<ImageSource>(dictionaryName, resourceKey)
            ?? throw new ArgumentException($"Resource '{resourceKey}' not found in {dictionaryName}");
    }

    public static ImageSource GetIcon(string resourceKey)
    {
        string dictionaryName = ICONS_DICTIONARY;

        return TryGetResource<ImageSource>(dictionaryName, resourceKey)
            ?? throw new ArgumentException($"Resource '{resourceKey}' not found in {dictionaryName}");
    }

    public static string GetFullImagePath(this ImageSource imageSource)
    {
        if (imageSource is BitmapImage image)
        {
            return Path.Combine(AppContext.BaseDirectory, image.UriSource.AbsolutePath.Trim('/', '\\'));
        }

        return string.Empty;
    }

    public static Style GetFlyoutStyle(string resourceKey)
    {
        string dictionaryName = FLYOUT_STYLES_DICTIONARY;

        return TryGetResource<Style>(dictionaryName, resourceKey)
            ?? throw new ArgumentException($"Resource '{resourceKey}' not found in {dictionaryName}");
    }

    private static string GetThemeKey(ElementTheme theme)
    {
        return theme switch
        {
            ElementTheme.Light or ElementTheme.Dark => theme.ToString(),
            _ => Application.Current?.RequestedTheme.ToString() ?? string.Empty
        };
    }

    private static ResourceDictionary? GetResourceDictionary(string dictionaryName)
    {
        return Application.Current?.Resources
            .MergedDictionaries.FirstOrDefault(md => md.Source.AbsoluteUri.EndsWith(dictionaryName));
    }

    private static ResourceDictionary? GetResourceDictionary(string dictionaryName, ElementTheme theme)
    {
        ResourceDictionary? dictionary = GetResourceDictionary(dictionaryName);

        string themeKey = GetThemeKey(theme);

        return string.IsNullOrEmpty(themeKey) || dictionary?.ThemeDictionaries == null || !dictionary.ThemeDictionaries.ContainsKey(themeKey)
            ? dictionary
            : dictionary.ThemeDictionaries[themeKey] as ResourceDictionary;
    }

    private static TResource? TryGetResource<TResource>(string dictionaryName, string resourceKey)
    {
        ResourceDictionary? dictionary = GetResourceDictionary(dictionaryName);

        return dictionary.TryGetResource<TResource>(resourceKey);
    }

    private static TResource? TryGetResource<TResource>(string dictionaryName, string resourceKey, ElementTheme theme)
    {
        ResourceDictionary? dictionary = GetResourceDictionary(dictionaryName, theme);

        return dictionary.TryGetResource<TResource>(resourceKey);
    }

    private static TResource? TryGetResource<TResource>(this ResourceDictionary? dictionary, string resourceKey)
    {
        if (dictionary == null || !dictionary.ContainsKey(resourceKey))
        {
            return default;
        }

        return (TResource)dictionary[resourceKey];
    }
}