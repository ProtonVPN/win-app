/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Linq;
using ProtonVPN.UpdateServiceContract;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppServiceLogs;
using ProtonVPN.Update;

namespace ProtonVPN.UpdateService
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class UpdateHandler : IUpdateContract
    {
        private readonly ILogger _logger;
        private readonly object _callbackLock = new();
        private readonly List<IUpdateEventsContract> _callbacks = new();
        private readonly INotifyingAppUpdate _updater;

        public UpdateHandler(ILogger logger, INotifyingAppUpdate updater)
        {
            _updater = updater;
            _logger = logger;
            _updater.StateChanged += OnUpdaterStateChanged;
        }

        public Task CheckForUpdate(bool earlyAccess)
        {
            _updater.StartCheckingForUpdate(earlyAccess);

            return Task.CompletedTask;
        }

        public Task RegisterCallback()
        {
            lock (_callbackLock)
            {
                _callbacks.Add(OperationContext.Current.GetCallbackChannel<IUpdateEventsContract>());
            }

            return Task.CompletedTask;
        }

        public Task UnRegisterCallback()
        {
            _logger.Info<AppServiceLog>("Unregister callback requested");

            lock (_callbackLock)
            {
                _callbacks.Remove(OperationContext.Current.GetCallbackChannel<IUpdateEventsContract>());
            }

            return Task.CompletedTask;
        }

        public async Task Update(bool auto)
        {
            await _updater.StartUpdating(auto);
        }

        private void OnUpdaterStateChanged(object sender, IAppUpdateState e)
        {
            lock (_callbackLock)
            {
                foreach (IUpdateEventsContract callback in _callbacks.ToList())
                {
                    try
                    {
                        callback.OnStateChanged(new UpdateStateContract(
                            Map(e.ReleaseHistory),
                            e.Available,
                            e.Ready,
                            Map(e.Status),
                            e.FilePath,
                            e.FileArguments));
                    }
                    catch (Exception ex) when (ex.IsServiceCommunicationException())
                    {
                        _logger.Warn<AppServiceCommunicationFailedLog>($"Callback failed: {ex.Message}");
                        _callbacks.Remove(callback);
                    }
                    catch (TimeoutException)
                    {
                        _logger.Warn<AppServiceCommunicationFailedLog>("Callback timed out");
                    }
                }
            }
        }

        private ICollection<ReleaseContract> Map(IReadOnlyList<IRelease> list)
        {
            return list.Select(release => new ReleaseContract(
                release.Version,
                release.EarlyAccess,
                release.New,
                release.ChangeLog.ToList())).ToList();
        }

        private AppUpdateStatusContract Map(AppUpdateStatus status)
        {
            return (AppUpdateStatusContract) status;
        }
    }
}
