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
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using ProtonDrive.Downloader.Response;

namespace ProtonDrive.Downloader
{
    public partial class MainWindow
    {
        private const string FEED_URL = "https://proton.me/download/drive/windows/version.json";

        private readonly HttpClient _httpClient;

        public MainWindow()
        {
            _httpClient = new HttpClient(new CertificateHandler());
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(GetUserAgent());

            InitializeComponent();
            Loaded += OnWindowLoadedAsync;
        }

        private async void OnWindowLoadedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                ReleaseResponse release = await GetLatestReleaseAsync();
                if (release != null)
                {
                    string pathToSave = GetPathToSave(release);
                    await DownloadAsync(release.File.Url, pathToSave);
                    if (File.Exists(pathToSave))
                    {
                        string hash = GetSHA512(pathToSave);
                        if (hash == release.File.Sha512CheckSum)
                        {
                            StartProcess(pathToSave, GetCommandLineArgs());
                        }
                        else
                        {
                            File.Delete(pathToSave);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            Close();
        }

        private string GetCommandLineArgs()
        {
            string result = "/qn";
            string installPath = GetInstallPath();
            if (!string.IsNullOrEmpty(installPath))
            {
                result += $" APPDIR=\"{installPath}\"";
            }

            return result;
        }

        private string GetPathToSave(ReleaseResponse release)
        {
            Uri fileUri = new(release.File.Url);
            return Path.Combine(Path.GetTempPath(), Path.GetFileName(fileUri.AbsolutePath));
        }

        private async Task<ReleaseResponse> GetLatestReleaseAsync()
        {
            HttpResponseMessage response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, FEED_URL));
            string body = await response.Content.ReadAsStringAsync();
            ReleasesResponse releasesResponse = JsonConvert.DeserializeObject<ReleasesResponse>(body);

            return releasesResponse?.Releases.Where(r => r.CategoryName == "Stable").MaxBy(GetVersion);
        }

        private Version GetVersion(ReleaseResponse response)
        {
            return Version.TryParse(response.Version, out Version version) ? version : new();
        }

        private async Task DownloadAsync(string url, string filename)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            byte[] buffer = new byte[8192];
            long totalBytesRead = 0;
            long totalBytes = response.Content.Headers.ContentLength ?? -1;

            while (true)
            {
                int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                {
                    break;
                }

                await fileStream.WriteAsync(buffer, 0, bytesRead);

                totalBytesRead += bytesRead;
                if (totalBytesRead > 0)
                {
                    double percentage = (double)totalBytesRead / totalBytes * 100;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Value = percentage;
                    });
                }
            }
        }

        private string GetSHA512(string filePath)
        {
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using SHA512 sha512 = SHA512.Create();
            byte[] hashBytes = sha512.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private void StartProcess(string path, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                UseShellExecute = false,
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
        }

        private string GetInstallPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            return args.Length >= 1 ? args[1] : string.Empty;
        }

        private string GetUserAgent()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
            return $"ProtonVPNInstaller/{version} ({Environment.OSVersion})";
        }
    }
}