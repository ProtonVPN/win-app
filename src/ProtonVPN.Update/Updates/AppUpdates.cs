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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Files;
using ProtonVPN.Update.Files.Downloadable;
using ProtonVPN.Update.Files.Launchable;
using ProtonVPN.Update.Files.UpdatesDirectory;
using ProtonVPN.Update.Files.Validatable;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Storage;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Performs app update related operations.
    /// Provides release history, checks for update, downloads, validates and starts update.
    /// </summary>
    public class AppUpdates : IAppUpdates
    {
        private readonly IReleaseStorage _releaseStorage;
        private readonly IUpdatesDirectory _updatesDirectory;
        private readonly FileLocation _fileLocation;
        private readonly IDownloadableFile _downloadable;
        private readonly IFileValidator _fileValidator;
        private readonly ILaunchableFile _launchable;

        public AppUpdates(IAppUpdateConfig config, ILaunchableFile launchableFile, ILogger logger)
        {
            Ensure.NotNull(config, nameof(config));

            config.Validate();

            _releaseStorage =
                new OrderedReleaseStorage(
                    new SafeReleaseStorage(
                        new WebReleaseStorage(config, logger)));

            _updatesDirectory = 
                new SafeUpdatesDirectory(
                    new UpdatesDirectory(config.UpdatesPath, config.CurrentVersion)
                );

            _fileLocation = new FileLocation(_updatesDirectory.Path);

            _downloadable = 
                new SafeDownloadableFile( 
                    new DownloadableFile(config.HttpClient));

            _fileValidator =
                new SafeFileValidator(
                    new CachedFileValidator(
                        new FileValidator()));

            _launchable = new SafeLaunchableFile(launchableFile);
        }

        public void Cleanup()
        {
            _updatesDirectory.Cleanup();
        }

        internal async Task<IReadOnlyList<Release>> ReleaseHistory(bool earlyAccess)
        {
            IEnumerable<Release> releases = await _releaseStorage.Releases();
            return releases.ToList();
        }

        internal async Task Download(Release release)
        {
            await _downloadable.Download(release.File.Url, FilePath(release));
        }

        internal async Task<bool> Valid(Release release)
        {
            return await _fileValidator.Valid(FilePath(release), release.File.Sha512CheckSum);
        }

        internal Task StartUpdate(Release release)
        {
            _launchable.Launch(FilePath(release), release.File.Arguments);
            return Task.CompletedTask;
        }

        internal string FilePath(Release release) => _fileLocation.Path(release.File.Url);
    }
}