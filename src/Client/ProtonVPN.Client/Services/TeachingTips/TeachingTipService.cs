/*
 * Copyright (c) 2025 Proton AG
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

using System.Collections.Concurrent;

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.TeachingTips;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Services.TeachingTips;

public class TeachingTipService : ITeachingTipService
{
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<TeachingTipKey, TeachingTipRegistration> _registrations = new();

    public TeachingTipService(IUIThreadDispatcher uiThreadDispatcher, ILogger logger)
    {
        _uiThreadDispatcher = uiThreadDispatcher;
        _logger = logger;
    }

    public bool Register(TeachingTipKey key, Action show, Action hide)
    {
        TeachingTipRegistration registration = new(key, show, hide);

        if (_registrations.TryAdd(key, registration))
        {
            _logger.Info<AppLog>($"TeachingTip '{key}' registered.");

            return true;
        }

        return false;
    }

    public bool Unregister(TeachingTipKey key)
    {
        if (_registrations.TryRemove(key, out _))
        {
            _logger.Info<AppLog>($"TeachingTip '{key}' unregistered.");

            return true;
        }

        return false;
    }

    public bool TryShow(TeachingTipKey key, Action? onAction = null, Action? onDismiss = null)
    {
        if (!_registrations.TryGetValue(key, out TeachingTipRegistration? registration))
        {
            _logger.Debug<AppLog>($"TeachingTip '{key}' is not registered. Cannot show.");
            return false;
        }

        registration.OnAction = onAction;
        registration.OnDismiss = onDismiss;

        _uiThreadDispatcher.TryEnqueue(() =>
        {
            registration.Show();
            _logger.Info<AppLog>($"TeachingTip '{key}' shown.");
        });

        return true;
    }
    
    public void InvokeAction(TeachingTipKey key)
    {
        if (_registrations.TryGetValue(key, out TeachingTipRegistration? registration))
        {
            _uiThreadDispatcher.TryEnqueue(() =>
            {
                // First call hide cause action could trigger teaching tip to be dismissed.
                registration.Hide();
                registration.OnAction?.Invoke();
                _logger.Debug<AppLog>($"TeachingTip '{key}' action invoked.");
            });
        }
    }

    public void InvokeDismiss(TeachingTipKey key)
    {
        if (_registrations.TryGetValue(key, out TeachingTipRegistration? registration))
        {
            _uiThreadDispatcher.TryEnqueue(() =>
            {                  
                registration.Hide();
                registration.OnDismiss?.Invoke();
                _logger.Debug<AppLog>($"TeachingTip '{key}' dismissed.");
            });
        }
    }
}