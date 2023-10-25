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
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Common.Legacy.OS.Services;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service;

internal partial class VpnService : ServiceBase
{
    public CancellationToken CancellationToken { get; private set; }

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IOsProcesses _osProcesses;
    private readonly IVpnConnection _vpnConnection;
    private readonly Ipv6 _ipv6;
    private bool _isConnected;
    private IGrpcServer _grpcServer;

    public VpnService(
        ILogger logger,
        IIssueReporter issueReporter,
        IStaticConfiguration staticConfig,
        IOsProcesses osProcesses,
        IVpnConnection vpnConnection,
        Ipv6 ipv6,
        IGrpcServer grpcServer)
    {
        _logger = logger;
        _issueReporter = issueReporter;
        _staticConfig = staticConfig;
        _osProcesses = osProcesses;
        _vpnConnection = vpnConnection;
        _ipv6 = ipv6;
        _grpcServer = grpcServer;
        _vpnConnection.StateChanged += OnVpnStateChanged;

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;

        InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
        try
        {
            _grpcServer.CreateAndStart();
        }
        catch (Exception ex)
        {
            _logger.Error<AppServiceStartFailedLog>("An error occurred when starting VPN Service.", ex);
            LogEvent($"OnStart: {ex}");
            _issueReporter.CaptureError(ex);
        }
    }

    protected override async void OnStop()
    {
        try
        {
            _logger.Info<AppServiceStopLog>("Service is stopping");
            LogEvent("Service is stopping");

            _vpnConnection.Disconnect();
            StopWireGuardService();

            if (!_ipv6.Enabled)
            {
                _ipv6.Enable();
            }

            await _grpcServer?.KillAsync();
        }
        catch (Exception ex)
        {
            _logger.Error<AppServiceStopFailedLog>("An error occurred when stopping VPN Service.", ex);
            LogEvent($"OnStop: {ex}");
            _issueReporter.CaptureError(ex);
        }
        finally
        {
            _cancellationTokenSource.Cancel();
        }
    }

    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
        _logger.Info<OperatingSystemLog>($"Power status changed to {powerStatus}");
        if (powerStatus == PowerBroadcastStatus.ResumeSuspend && _isConnected)
        {
            _logger.Info<ConnectionLog>("Disconnecting due to resume from sleep.");
            _vpnConnection.Disconnect(VpnError.Unknown);
        }

        return true;
    }

    private void StopWireGuardService()
    {
        SystemService wireGuardService = new(_staticConfig.WireGuard.ServiceName, _osProcesses);
        if (wireGuardService.Running())
        {
            wireGuardService.StopAsync(new CancellationToken()).Wait();
        }
    }

    private void LogEvent(string message)
    {
        try
        {
            EventLog.WriteEntry(message.Replace('%', '_'));
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception)
        {
        }
    }

    private void OnVpnStateChanged(object sender, Common.Legacy.EventArgs<VpnState> e)
    {
        _isConnected = e.Data.Status == VpnStatus.Connected;
    }
}