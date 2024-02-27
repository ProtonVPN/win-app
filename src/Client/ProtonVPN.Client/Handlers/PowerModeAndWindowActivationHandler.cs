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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.PowerEvents.Contracts;

namespace ProtonVPN.Client.Handlers;

public class PowerModeAndWindowActivationHandler : IHandler, IEventMessageReceiver<ApplicationStartedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IAuthCertificateManager _authCertificateManager;
    private readonly IUserAuthenticator _userAuthenticator;

    public PowerModeAndWindowActivationHandler(ILogger logger,
        ISettings settings,
        IVpnServiceCaller vpnServiceCaller,
        IAuthCertificateManager authCertificateManager,
        IUserAuthenticator userAuthenticator,
        IPowerEventNotifier powerEventNotifier)
    {
        _logger = logger;
        _settings = settings;
        _vpnServiceCaller = vpnServiceCaller;
        _authCertificateManager = authCertificateManager;
        _userAuthenticator = userAuthenticator;

        powerEventNotifier.OnResume += OnResume;
    }

    private void OnResume(object? sender, EventArgs e)
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            _logger.Info<AppLog>("Resuming from sleep while logged in");
            RepeatVpnStateAndRequestNewCertificate();
        }
        else
        {
            _logger.Info<AppLog>("Resuming from sleep while logged out");
        }
    }

    private void RepeatVpnStateAndRequestNewCertificate()
    {
        _vpnServiceCaller.RepeatStateAsync();

        if (_settings.IsPortForwardingEnabled)
        {
            _vpnServiceCaller.RepeatPortForwardingStateAsync();
        }

        _authCertificateManager.RequestNewCertificateAsync();
    }

    public void Receive(ApplicationStartedMessage message)
    {
        App.MainWindow.Activated += OnActivationStateChange;
    }

    private void OnActivationStateChange(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState is not WindowActivationState.Deactivated &&
            _userAuthenticator.IsLoggedIn)
        {
            _logger.Debug<AppLog>("Window activated while logged in");
            RepeatVpnStateAndRequestNewCertificate();
        }
    }
}