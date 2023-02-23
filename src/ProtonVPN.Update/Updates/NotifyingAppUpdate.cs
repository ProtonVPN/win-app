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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppUpdateLogs;
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Performs series of asynchronous update checking, downloading and verifying operations
    /// and notifies about the state change.
    /// </summary>
    internal class NotifyingAppUpdate : INotifyingAppUpdate
    {
        private readonly CoalescingAction _checkForUpdate;

        private IAppUpdate _update;
        private ILogger _logger;
        private AppUpdateStatus _status = AppUpdateStatus.None;
        private bool _earlyAccess;
        private volatile bool _requestedEarlyAccess;

        public NotifyingAppUpdate(IAppUpdate update, ILogger logger)
        {
            _update = update;
            _logger = logger;

            _checkForUpdate = new CoalescingAction(SafeCheckForUpdate);
        }

        public event EventHandler<IAppUpdateState> StateChanged;

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

        private void HandleFailure()
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
            _logger.Error<AppUpdateLog>($"An update failed with status '{_status}' (File path: {_update.FilePath}).");
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            OnStateChanged(_status);
        }

        private void OnStateChanged(AppUpdateStatus status)
        {
            var eventArgs = new AppUpdateState(_update, status);
            StateChanged?.Invoke(this, eventArgs);
        }
    }
}
