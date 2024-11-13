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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Registries.Contracts;

namespace ProtonVPN.Client.Handlers;

public class AutoStartupHandler : IHandler, IEventMessageReceiver<SettingChangedMessage>
{
    private const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IRegistryEditor _registryEditor;

    private readonly Lazy<RegistryUri> _registryUri;
    private readonly Lazy<RegistryUri> _oldRegistryUri;
    private readonly Lazy<string> _clientLauncherExePath;

    public AutoStartupHandler(IConfiguration config,
        ILogger logger,
        ISettings settings,
        IRegistryEditor registryEditor)
    {
        _config = config;
        _logger = logger;
        _settings = settings;
        _registryEditor = registryEditor;

        _registryUri = new Lazy<RegistryUri>(() => RegistryUri.CreateCurrentUserUri(RUN_KEY, App.APPLICATION_NAME));
        _oldRegistryUri = new Lazy<RegistryUri>(() => RegistryUri.CreateCurrentUserUri(RUN_KEY, _config.ClientName));
        _clientLauncherExePath = new Lazy<string>(() => _config.ClientLauncherExePath);

        Initialize();
    }

    private void Initialize()
    {
        DeleteOldRegistryKey();

        InvalidateAutoLaunch();

        bool isAutoStartupEnabledOnRegistry = IsAutoStartupEnabledOnRegistry();
        if (_settings.IsAutoLaunchEnabled != isAutoStartupEnabledOnRegistry)
        {
            _settings.IsAutoLaunchEnabled = isAutoStartupEnabledOnRegistry;
        }
    }

    /// <summary>
    /// TODO: Remove this method before releasing to Beta
    /// When moving to v4, auto start registry key was added using the Client name (ProtonVPN.Client) rather than the Application name (Proton VPN)
    /// We changed the key name but internal users that already updated to v4 might still have the old one, so we need to remove it.
    /// Future beta users won't have this issue so we can remove this method before releasing to beta.
    /// </summary>
    private void DeleteOldRegistryKey()
    {
        _registryEditor.Delete(_oldRegistryUri.Value);
    }

    private bool IsAutoStartupEnabledOnRegistry()
    {
        try
        {
            return _registryEditor.ReadObject(_registryUri.Value) is not null;
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to read auto startup value from the registry.", e);
            return false;
        }
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
            _registryEditor.WriteString(_registryUri.Value, _clientLauncherExePath.Value);
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
            _registryEditor.Delete(_registryUri.Value);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to disable auto startup.", e);
        }
    }
}