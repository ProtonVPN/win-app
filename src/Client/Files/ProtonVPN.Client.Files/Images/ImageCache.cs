﻿/*
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

using System.Security.Cryptography;
using System.Text;
using System.Linq;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Files.Contracts.Images;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy.OS.Net.Http;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using File = System.IO.File;

namespace ProtonVPN.Client.Files.Images;

public class ImageCache : IImageCache
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly IHttpClient _httpClient;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ImageCache(ILogger logger,
        IConfiguration config,
        IFileDownloadHttpClientFactory fileDownloadHttpClientFactory)
    {
        _logger = logger;
        _config = config;
        _httpClient = fileDownloadHttpClientFactory.GetHttpClientWithTlsPinning();
    }

    public async Task<CachedImage?> GetAsync(string folder, string? downloadUrl)
    {
        try
        {
            if (downloadUrl != null)
            {
                string? localPath = await GetLocalPathOrDownloadAsync(folder, downloadUrl);
                if (localPath != null)
                {
                    return new CachedImage() { LocalPath = localPath };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("An error occurred when fetching or downloading an image.", ex);
        }

        return null;
    }

    private async Task<string?> GetLocalPathOrDownloadAsync(string folder, string downloadUrl)
    {
        string localPath = GenerateFullFilePath(folder, downloadUrl);

        if (await DoesFileExistAsync(localPath))
        {
            return localPath;
        }

        return await DownloadAsync(downloadUrl, localPath);
    }

    private string GenerateFullFilePath(string folder, string downloadUrl)
    {
        return Path.Combine(_config.ImageCacheFolder, folder, GenerateFileName(downloadUrl));
    }

    private string GenerateFileName(string downloadUrl)
    {
        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(downloadUrl));
        StringBuilder sb = new();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
    }

    private async Task<bool> DoesFileExistAsync(string localPath)
    {
        await _semaphore.WaitAsync();
        try
        {
            return File.Exists(localPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string?> DownloadAsync(string downloadUrl, string localPath)
    {
        await _semaphore.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
            return await DownloadImageAsync(downloadUrl, localPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<string?> DownloadImageAsync(string downloadUrl, string localPath)
    {
        try
        {
            using IHttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);
            if (response.IsSuccessStatusCode && response.ContentMediaType.StartsWithIgnoringCase("image/"))
            {
                using Stream contentStream = await response.Content.ReadAsStreamAsync();
                using FileStream fileStream = new(localPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await contentStream.CopyToAsync(fileStream);
                return localPath;
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>($"Failed to download image using URL {SanitizeForLog(downloadUrl)}", ex);
        }

        return null;
    }

    [Obsolete("Ideally we should remove this method and transform all mappers into Async")]
    public CachedImage? Get(string folder, string? downloadUrl)
    {
        Task<CachedImage?> task = GetAsync(folder, downloadUrl);
        task.Wait();
        return task.Result;
    }

    public IList<CachedImage> GetAllFromFolder(string folder)
    {
        try
        {
            string? fullPath = Path.Combine(_config.ImageCacheFolder, folder);
            return Directory.Exists(fullPath)
                ? Directory.GetFiles(fullPath).Select(path => new CachedImage() { LocalPath = path }).ToList()
                : [];
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Failed to fetch all cached image paths.", ex);
        }

        return [];
    }

    public void Delete(CachedImage cachedImage)
    {
        try
        {
            if (File.Exists(cachedImage.LocalPath))
            {
                File.Delete(cachedImage.LocalPath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>($"Failed to delete the cached image '{cachedImage.LocalPath}'.", ex);
        }
    }
    /// <summary>
    /// Sanitizes user input for safe logging by removing newlines and control characters.
    /// </summary>
    private static string SanitizeForLog(string input)
    {
        if (input == null)
            return string.Empty;
        // Remove CR, LF, and other control characters
        var sanitized = input.Replace("\r", "").Replace("\n", "");
        // Optionally, remove other non-printable characters
        sanitized = new string(sanitized.Where(c => !char.IsControl(c)).ToArray());
        return sanitized;
    }
}
