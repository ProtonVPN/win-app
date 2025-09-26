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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace ProtonVPN.Client.Common.UI.Extensions;

public static class PathExtensions
{
    public static string GetFolderName(this string path)
    {
        try
        {
            return Directory.Exists(path)
                ? Path.GetFileName(path).Trim()
                : Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty).Trim();
        }
        catch (Exception) { }

        return string.Empty;
    }

    public static string GetAppName(this string filePath)
    {
        try
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            string? name = versionInfo.ProductName?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                name = versionInfo.FileDescription?.Trim();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileNameWithoutExtension(filePath).Trim();
            }

            return name;
        }
        catch (FileNotFoundException) { }
        catch (ArgumentException) { }
        catch (Exception) { }

        return string.Empty;
    }

    public static async Task<ImageSource?> GetAppIconAsync(this string filePath)
    {
        try
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            StorageItemThumbnail iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 20);
            BitmapImage image = new();
            image.SetSource(iconThumbnail);
            return image;
        }
        catch (Exception) { }

        return null;
    }
}