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
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.Common.Legacy.Threading;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Performs series of asynchronous update checking, downloading and verifying operations
    /// and notifies about the state change.
    /// </summary>
    public class NotifyingAppUpdate : INotifyingAppUpdate
    {
        private readonly CoalescingAction _checkForUpdate;

        private IAppUpdate _update;
        private readonly ILogger _logger;
        private AppUpdateStatus _status = AppUpdateStatus.None;
        private bool _earlyAccess;
        private volatile bool _requestedEarlyAccess;

        public NotifyingAppUpdate(IAppUpdate update, ILogger logger)
        {
            _update = update;
            _logger = logger;

            _checkForUpdate = new CoalescingAction(SafeCheckForUpdate);
        }

        public event EventHandler<AppUpdateStateContract> StateChanged;

        public void StartCheckingForUpdate(bool earlyAccess)
        {
            if (_checkForUpdate.Running)
            {
                if (_requestedEarlyAccess == earlyAccess)
                {
                    return;
                }

                _checkForUpdate.Cancel();
            }

            _requestedEarlyAccess = earlyAccess;
            _checkForUpdate.Run();
        }

        public async Task StartUpdating(bool auto)
        {
            await _update.Started(auto);

            // The state change to Updating triggers the app to exit.
            // State is changed to Updating only if update has been successfully started (not raised an exception).
            OnStateChanged(AppUpdateStatus.Updating);
        }

        private async Task SafeCheckForUpdate(CancellationToken cancellationToken)
        {
            try
            {
                await UnsafeCheckForUpdate(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                HandleCancellation();
            }
            catch (AppUpdateException)
            {
                HandleFailure();
            }
            catch (Exception e)
            {
                HandleFailure(e);
            }
        }

        private async Task UnsafeCheckForUpdate(CancellationToken cancellationToken)
        {
            _logger.Info<AppUpdateCheckLog>("Checking for updates.");
            _earlyAccess = _requestedEarlyAccess;
            _status = AppUpdateStatus.Checking;

            HandleSuccess(_update.CachedLatest(_earlyAccess), cancellationToken);
            HandleSuccess(await _update.Latest(_earlyAccess), cancellationToken);

            if (_update.Available)
            {
                string fileName = Path.GetFileNameWithoutExtension(_update.FilePath);
                _logger.Info<AppUpdateLog>($"An update is available (File name: {fileName}).");
                HandleSuccess(await _update.Validated(), cancellationToken);

                if (!_update.Ready)
                {
                    _logger.Info<AppUpdateLog>($"The latest update is being downloaded (File name: {fileName}).");
                    _status = AppUpdateStatus.Downloading;
                    OnStateChanged();

                    HandleSuccess(await _update.Downloaded(), cancellationToken);
                    HandleSuccess(await _update.Validated(), cancellationToken);

                    if (_update.Ready)
                    {
                        _logger.Info<AppUpdateLog>("The latest update was successfully " +
                            $"downloaded and validated (File name: {fileName}).");
                    }
                    else
                    {
                        _logger.Error<AppUpdateLog>("The latest update failed to download " +
                            $"(File path: {_update.FilePath}).");
                        _status = AppUpdateStatus.DownloadFailed;
                        OnStateChanged();

                        return;
                    }
                }
            }

            _status = _update.Ready ? AppUpdateStatus.Ready : AppUpdateStatus.None;
            OnStateChanged();
        }

        private void HandleSuccess(IAppUpdate update, CancellationToken cancellationToken)
        {
            _update = update;
            cancellationToken.ThrowIfCancellationRequested();

            OnStateChanged();
        }

        private void HandleCancellation()
        {
            _status = AppUpdateStatus.None;
            OnStateChanged();
        }

        private void HandleFailure(Exception e = null)
        {
            switch (_status)
            {
                case AppUpdateStatus.Checking:
                    _status = AppUpdateStatus.CheckFailed;
                    break;
                case AppUpdateStatus.Downloading:
                    _status = AppUpdateStatus.DownloadFailed;
                    break;
                default:
                    _status = AppUpdateStatus.None;
                    break;
            }
            _logger.Error<AppUpdateLog>($"An update failed with status '{_status}' (File path: {_update.FilePath}).", e);
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            OnStateChanged(_status);
        }

        private void OnStateChanged(AppUpdateStatus status)
        {
            AppUpdateStateContract eventArgs = new()
            {
                IsAvailable = _update.Available,
                FileArguments = _update.FileArguments,
                FilePath = _update.FilePath,
                Version = _update.Version,
                IsReady = _update.Ready,
                Status = status,
                ReleaseHistory = Map(_update.ReleaseHistory()),
            };
            StateChanged?.Invoke(this, eventArgs);
        }

        private IReadOnlyList<ReleaseContract> Map(IReadOnlyList<IRelease> releases)
        {
            return releases.Select(release => new ReleaseContract
            {
                ChangeLog = release.ChangeLog,
                IsEarlyAccess = release.IsEarlyAccess,
                ReleaseDate = release.ReleaseDate,
                IsNew = release.IsNew,
                Version = release.Version
            }).ToList();
        }
    }
}