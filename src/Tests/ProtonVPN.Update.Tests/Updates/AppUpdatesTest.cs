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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Tests.Common;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Files.Launchable;
using ProtonVPN.Update.Updates;

// ReSharper disable ObjectCreationAsStatement

namespace ProtonVPN.Update.Tests.Updates
{
    [TestClass]
    public class AppUpdatesTest
    {
        private ILogger _logger;
        private ILaunchableFile _launchableFile;
        private IHttpClient _httpClient;
        private IFeedUrlProvider _feedUrlProvider;
        private DefaultAppUpdateConfig _config;
        private IEnumerable<Uri> _feedUrls = new List<Uri>()
        {
            new Uri("http://127.0.0.1/windows-releases.json"),
            new Uri("http://127.0.0.1/win-update.json")
        };

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _launchableFile = Substitute.For<ILaunchableFile>();
            _httpClient = Substitute.For<IHttpClient>();
            _feedUrlProvider = Substitute.For<IFeedUrlProvider>();
            _feedUrlProvider.GetFeedUrls().Returns(_feedUrls);
            _config = new DefaultAppUpdateConfig
            {
                HttpClient = _httpClient,
                FeedUriProvider = _feedUrlProvider,
                UpdatesPath = "Updates",
                CurrentVersion = new Version(),
                EarlyAccessCategoryName = "EarlyAccess"
            };
        }

        private IAppUpdates AppUpdates(Version version, [CallerMemberName] string updatesPath = null)
        {
            _config.CurrentVersion = version;
            _config.UpdatesPath = updatesPath;
            return AppUpdates();
        }

        private IAppUpdates AppUpdates()
        {
            return new AppUpdates(_config, _launchableFile, _logger);
        }

        #endregion

        #region Test: AppUpdates

        [TestMethod]
        public void AppUpdates_ShouldNotTrow()
        {
            Action f = () => new AppUpdates(_config, _launchableFile, _logger);

            f.Should().NotThrow<Exception>();
        }

        [TestMethod]
        public void AppUpdates_ShouldTrow_WhenConfig_IsNull()
        {
            Action f = () => new AppUpdates(null, _launchableFile, _logger);

            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AppUpdates_ShouldTrow_WhenHttpClient_IsNull()
        {
            _config.HttpClient = null;
            Action f = () => new AppUpdates(_config, _launchableFile, _logger);

            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AppUpdates_ShouldTrow_WhenCurrentVersion_IsNull()
        {
            _config.CurrentVersion = null;
            Action f = () => new AppUpdates(_config, _launchableFile, _logger);

            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AppUpdates_ShouldTrow_WhenFeedUri_IsNull()
        {
            _config.FeedUriProvider = null;
            Action f = () => new AppUpdates(_config, _launchableFile, _logger);

            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AppUpdates_ShouldTrow_WhenUpdatesPath_IsNull()
        {
            _config.UpdatesPath = null;
            Action f = () => new AppUpdates(_config, _launchableFile, _logger);

            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void AppUpdates_ShouldTrow_WhenUpdatesPath_IsEmpty()
        {
            _config.UpdatesPath = "";
            Action f = () => new AppUpdates(_config, _launchableFile, _logger);

            f.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Test: Cleanup

        [TestMethod]
        public void Cleanup_ShouldDelete_Subdirectories_FromDownloadsDirectory()
        {
            string updatesPath = TestConfig.GetFolderPath();
            Directory.CreateDirectory(Path.Combine(updatesPath, "2.2.2"));
            Directory.CreateDirectory(Path.Combine(updatesPath, "Some", "Another"));
            CopyFile("Empty file.txt", updatesPath);
            CopyFile("Empty file.txt", Path.Combine(updatesPath, "Some", "2nd"));

            IAppUpdates updater = AppUpdates(new Version(1, 2, 0), updatesPath);
            updater.Cleanup();

            string[] directories = Directory.GetDirectories(updatesPath);
            Directory.Delete(updatesPath, true);
            directories.Should().BeEmpty();
        }

        [TestMethod]
        public void Cleanup_ShouldDelete_AllNotExeFiles_FromDownloadsDirectory()
        {
            string updatesPath = TestConfig.GetFolderPath();
            CopyFile("Empty file.txt", updatesPath);
            CopyFile("Empty file.txt", updatesPath, "Without extension");
            CopyFile("ProtonVPN_win_v1.0.0.exe", updatesPath, "Some.1");
            CopyFile("ProtonVPN_win_v2.0.0.exe", updatesPath, "Later version not exe.dll");

            IAppUpdates updater = AppUpdates(new Version(1, 2, 0), updatesPath);
            updater.Cleanup();

            string[] files = Directory.GetFiles(updatesPath, "*", SearchOption.AllDirectories);
            Directory.Delete(updatesPath, true);
            files.Should().BeEmpty();
        }

        [TestMethod]
        public void Cleanup_ShouldDelete_OutdatedExeFiles_FromDownloadsDirectory()
        {
            string updatesPath = TestConfig.GetFolderPath();
            CopyFile("Empty file.txt", updatesPath, "Unknown.exe");
            CopyFile("ProtonVPN_win_v1.0.0.exe", updatesPath);
            CopyFile("ProtonVPN_win_v1.5.0.exe", updatesPath);
            CopyFile("ProtonVPN_win_v1.5.1.exe", updatesPath);

            IAppUpdates updater = AppUpdates(new Version(1, 5, 1), updatesPath);

            updater.Cleanup();

            string[] files = Directory.GetFiles(updatesPath, "*", SearchOption.AllDirectories);
            Directory.Delete(updatesPath, true);

            files.Should()
                .HaveCount(1)
                .And.Match(f => Path.GetFileName(f.First()) == "ProtonVPN_win_v1.5.1.exe");
        }

        #endregion

        #region Helpers

        private static void CopyFile(string sourcePath, string destPath, string newFilename = null)
        {
            if (!string.IsNullOrEmpty(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string filename = !string.IsNullOrEmpty(newFilename) ? newFilename : Path.GetFileName(sourcePath);
            string destFullPath = Path.Combine(destPath ?? "", filename ?? "");

            File.Copy(TestConfig.GetFolderPath(sourcePath), destFullPath);
        }

        #endregion
    }
}