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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Services.Bootstrapping.Activators;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.OperatingSystems.Registries.Contracts;

namespace ProtonVPN.Client.Handlers;

public class AutoStartupHandler : IHandler, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IAppStartupActivator _appStartupActivator;

    public AutoStartupHandler(
        ISettings settings,
        IAppStartupActivator appStartupActivator)
    {
        _settings = settings;
        _appStartupActivator = appStartupActivator;

        InvalidateAutoLaunch();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsAutoLaunchEnabled))
        {
            InvalidateAutoLaunch();
        }
    }

    private void InvalidateAutoLaunch()
    {
        if (_settings.IsAutoLaunchEnabled)
        {
            _appStartupActivator.Register();
        }
        else
        {
            _appStartupActivator.Unregister();
        }
    }
}