/*
 * Copyright (c) 2026 Proton AG
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

using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Handlers.Bases;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.PowerEvents.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Handlers;

public class PowerModeAndWindowActivationHandler : IHandler,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private readonly IServersUpdater _serversUpdater;
    private readonly IConfiguration _config;

    public PowerModeAndWindowActivationHandler(ILogger logger,
        ISettings settings,
        IVpnServiceCaller vpnServiceCaller,
        IConnectionCertificateManager connectionCertificateManager,
        IUserAuthenticator userAuthenticator,
        IPowerEventNotifier powerEventNotifier,
        IConnectionManager connectionManager,
        IVpnPlanUpdater vpnPlanUpdater,
        IServersUpdater serversUpdater,
        IConfiguration config)
    {
        _logger = logger;
        _settings = settings;
        _vpnServiceCaller = vpnServiceCaller;
        _connectionCertificateManager = connectionCertificateManager;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _vpnPlanUpdater = vpnPlanUpdater;
        _serversUpdater = serversUpdater;
        _config = config;

        powerEventNotifier.OnResume += OnResume;
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn || !message.IsMainWindowVisible)
        {
            return;
        }

        _logger.Debug<AppLog>("Main window activated while logged in");

        OnWindowActivationAsync().FireAndForget();
    }

    private void OnResume(object? sender, EventArgs e)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            _logger.Info<AppLog>("Resuming from sleep while logged out");
            return;
        }

        _logger.Info<AppLog>("Resuming from sleep while logged in");

        OnResumeAsync().FireAndForget();
    }

    private async Task OnWindowActivationAsync()
    {
        await RepeatServiceStateAsync();
        await RefreshPlanAndCertificateAsync();
    }

    private async Task OnResumeAsync()
    {
        // The service state is from before the sleep, and the reconnection decision below reads it.
        await RepeatServiceStateAsync();

        // Before reconnecting, the connect path never refreshes the plan and only renews an expired certificate.
        // The servers are refreshed too, so the reconnection does not pick a server that went down during the sleep.
        await RefreshWithinTimeoutAsync();

        if (_connectionManager.IsConnected)
        {
            await ReconnectAsync();
        }
    }

    private async Task ReconnectAsync()
    {
        try
        {
            await _connectionManager.ReconnectAsync(VpnTriggerDimension.Auto);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to reconnect after resuming from sleep.", e);
        }
    }

    private async Task RepeatServiceStateAsync()
    {
        await _vpnServiceCaller.RepeatStateAsync();

        if (_settings.IsPortForwardingEnabled)
        {
            await _vpnServiceCaller.RepeatPortForwardingStateAsync();
        }
    }

    // Best effort. Neither a slow nor a failing refresh may hold the tunnel down, the connection error
    // handler recovers from a stale certificate or plan on its own.
    private async Task RefreshWithinTimeoutAsync()
    {
        try
        {
            Task refreshTask = Task.WhenAll(
                RefreshPlanAndCertificateAsync(),
                _serversUpdater.UpdateAsync());

            if (!await refreshTask.WaitOrFireAndForgetAsync(_config.ResumeRefreshTimeout))
            {
                _logger.Warn<AppLog>("The plan, the certificate and the servers were not refreshed in time, reconnecting with the cached ones.");
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Failed to refresh the plan, the certificate and the servers after resuming from sleep.", e);
        }
    }

    private async Task RefreshPlanAndCertificateAsync()
    {
        await Task.WhenAll(
            _vpnPlanUpdater.UpdateAsync(),
            _connectionCertificateManager.RequestNewCertificateAsync());
    }
}