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

using System;
using System.IO;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ProtonVPN.Client.Common.UI.Converters;

public class CountryCodeToFlagImageSourceConverter : IValueConverter
{
    private const string ASSETS_FOLDER_PATH = "ms-appx:///ProtonVPN.Client.Common.UI/Assets/Flags/";
    private const string ASSETS_FILE_EXTENSION = ".svg";
    private const string ASSETS_PLACEHOLDER = "Placeholder";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string countryCode = value?.ToString() ?? string.Empty;

        return GetImageSource(countryCode);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private SvgImageSource GetImageSource(string countryCode)
    {
        Uri uri = BuildUri(countryCode);

        if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uri.LocalPath.TrimStart('/', '\\'))))
        {
            uri = BuildUri(ASSETS_PLACEHOLDER);
        }

        return new SvgImageSource(uri);
    }

    private Uri BuildUri(string resourceName)
    {
        return new(Path.Combine(ASSETS_FOLDER_PATH, resourceName + ASSETS_FILE_EXTENSION));
    }
}