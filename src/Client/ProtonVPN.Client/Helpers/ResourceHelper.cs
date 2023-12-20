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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace ProtonVPN.Client.Helpers;

public static class ResourceHelper
{
    public static Color GetColor(ElementTheme theme, string resourceKey)
    {
        string themeKey = theme switch
        {
            ElementTheme.Light or ElementTheme.Dark => theme.ToString(),
            _ => Application.Current.RequestedTheme.ToString()
        };

        ResourceDictionary? colorsDictionary = Application.Current.Resources
            .MergedDictionaries.FirstOrDefault(md => md.Source.AbsoluteUri.EndsWith("Styles/Colors.xaml"));

        ResourceDictionary? themeDictionary = colorsDictionary?.ThemeDictionaries[themeKey] as ResourceDictionary;

        if (themeDictionary == null || !themeDictionary.ContainsKey(resourceKey))
        {
            return default;
        }

        return (Color)themeDictionary[resourceKey];
    }

    public static ImageSource GetIllustration(string resourceKey)
    {
        ResourceDictionary? illustrationsDictionary = Application.Current.Resources
            .MergedDictionaries.FirstOrDefault(md => md.Source.AbsoluteUri.EndsWith("Styles/Illustrations.xaml"));

        if (illustrationsDictionary == null || !illustrationsDictionary.ContainsKey(resourceKey))
        {
            return default;
        }

        return (ImageSource)illustrationsDictionary[resourceKey];
    }

    public static ImageSource GetIcon(string resourceKey)
    {
        ResourceDictionary? iconsDictionary = Application.Current.Resources
            .MergedDictionaries.FirstOrDefault(md => md.Source.AbsoluteUri.EndsWith("Styles/Icons.xaml"));

        if (iconsDictionary == null || !iconsDictionary.ContainsKey(resourceKey))
        {
            return default;
        }

        return (ImageSource)iconsDictionary[resourceKey];
    }

    public static Style GetContentDialogStyle(string resourceKey)
    {
        ResourceDictionary? contentDialogDictionary = Application.Current.Resources
            .MergedDictionaries.FirstOrDefault(md => md.Source.AbsoluteUri.EndsWith("Styles/Controls/ContentDialogStyles.xaml"));

        if (contentDialogDictionary == null || !contentDialogDictionary.ContainsKey(resourceKey))
        {
            return default;
        }

        return (Style)contentDialogDictionary[resourceKey];
    }

    public static string GetFullImagePath(this ImageSource imageSource)
    {
        if (imageSource is BitmapImage image)
        {
            return Path.Combine(AppContext.BaseDirectory, image.UriSource.AbsolutePath.Trim('/', '\\'));
        }

        return string.Empty;
    }
}