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

using Microsoft.Win32;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Handlers;

public class AutoStartupHandler : IHandler, IEventMessageReceiver<SettingChangedMessage>
{
    private const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly ISettings _settings;

    public AutoStartupHandler(IConfiguration config, ILogger logger, ISettings settings)
    {
        _config = config;
        _logger = logger;
        _settings = settings;

        Initialize();
    }

    private void Initialize()
    {
        InvalidateAutoLaunch();

        bool isAutoStartupEnabledOnRegistry = IsAutoStartupEnabledOnRegistry();
        if (_settings.IsAutoLaunchEnabled != isAutoStartupEnabledOnRegistry)
        {
            _settings.IsAutoLaunchEnabled = isAutoStartupEnabledOnRegistry;
        }
    }

    private bool IsAutoStartupEnabledOnRegistry()
    {
        try
        {
            using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(RUN_KEY, false);
            return registryKey?.GetValue(_config.ClientName) is not null;
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to read auto startup value from the registry.", e);
            return false;
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName != nameof(ISettings.IsAutoLaunchEnabled))
        {
            return;
        }

        InvalidateAutoLaunch();
    }

    private void InvalidateAutoLaunch()
    {
        bool isAutoLaunchEnabled = _settings.IsAutoLaunchEnabled;
        if (isAutoLaunchEnabled)
        {
            EnableAutoLaunch();
        }
        else
        {
            DisableAutoLaunch();
        }
    }

    private void EnableAutoLaunch()
    {
        try
        {
            using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            registryKey?.SetValue(_config.ClientName, _config.ClientLauncherExePath);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to enable auto startup.", e);
        }
    }

    private void DisableAutoLaunch()
    {
        try
        {
            using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(RUN_KEY, true);
            registryKey?.DeleteValue(_config.ClientName);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to disable auto startup.", e);
        }
    }
}