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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.NRPT.Contracts;
using ProtonVPN.OperatingSystems.PowerEvents.Contracts;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProTun.Contracts;
using ProtonVPN.Service.Firewall;
using ProtonVPN.Service.StateMachine;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service;

internal partial class VpnService : ServiceBase
{
    public CancellationToken CancellationToken { get; private set; }

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IEnumerable<IVpnConnection> _vpnConnections;
    private readonly IIpv6 _ipv6;
    private readonly IGrpcServer _grpcServer;
    private readonly INrptInvoker _nrptInvoker;
    private readonly IProTunManager _proTunManager;
    private readonly INrptWatchdogScheduler _nrptWatchdogScheduler;
    private readonly INrptWatchdogStarter _nrptWatchdogStarter;
    private readonly IVpnConnectionStateMachine _vpnConnectionStateMachine;

    public VpnService(
        ILogger logger,
        IIssueReporter issueReporter,
        IStaticConfiguration staticConfig,
        IEnumerable<IVpnConnection> vpnConnections,
        IIpv6 ipv6,
        IGrpcServer grpcServer,
        IPowerEventNotifier powerEventNotifier,
        INrptInvoker nrptInvoker,
        IProTunManager proTunManager,
        INrptWatchdogScheduler nrptWatchdogScheduler,   
        INrptWatchdogStarter nrptWatchdogStarter,
        IVpnConnectionStateMachine vpnConnectionStateMachine)
    {
        _logger = logger;
        _issueReporter = issueReporter;
        _staticConfig = staticConfig;
        _vpnConnections = vpnConnections;
        _ipv6 = ipv6;
        _grpcServer = grpcServer;
        _nrptInvoker = nrptInvoker;
        _proTunManager = proTunManager;
        _nrptWatchdogScheduler = nrptWatchdogScheduler;
        _nrptWatchdogStarter = nrptWatchdogStarter;
        _vpnConnectionStateMachine = vpnConnectionStateMachine;

        powerEventNotifier.OnResume += OnPowerEventResume;
        _grpcServer.InvokingServiceStop += OnInvokingServiceStop;

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        CanHandleSessionChangeEvent = true;

        AutoLog = false; // To disable the event logs "PowerEvent handled successfully by the service."

        InitializeComponent();
    }

    private void OnInvokingServiceStop(object? sender, EventArgs e)
    {
        Stop();
    }

    protected override void OnSessionChange(SessionChangeDescription changeDescription)
    {
        _logger.Info<AppServiceStopLog>($"Session changed, reason: {changeDescription.Reason}");

        if (changeDescription.Reason == SessionChangeReason.SessionLogoff)
        {
            _logger.Info<AppServiceStopLog>("Stopping the service due to SessionLogoff.");
            Stop();
        }

        base.OnSessionChange(changeDescription);
    }

    protected override void OnStart(string[] args)
    {
        LogEvent("Service is starting");
        try
        {
            if (!IsBfeServiceRunningAndEnabled())
            {
                _vpnConnectionStateMachine.ReportDisconnected(VpnError.BaseFilteringEngineServiceNotRunning);
                return;
            }

            _grpcServer.CreateAndStart();
            _proTunManager.InitializeAsync().FireAndForget();

            TriggerDisconnectAsync().FireAndForget();
            _nrptInvoker.DeleteRule();
            _nrptWatchdogScheduler.Schedule();
            _nrptWatchdogStarter.Start();
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

            await _vpnConnectionStateMachine.DisconnectAsync();

            if (!_ipv6.IsEnabled)
            {
                _ipv6.Enable(_ipv6.VpnProtocol);
            }

            await _grpcServer.StopAsync();
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

    private async Task TriggerDisconnectAsync()
    {
        foreach (IVpnConnection vpnConnection in _vpnConnections)
        {
            await vpnConnection.DisconnectAsync();
        }

        _vpnConnectionStateMachine.ReportDisconnected(VpnError.None);
    }

    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
        _logger.Debug<OperatingSystemLog>($"Power status changed to {powerStatus}");
        if (powerStatus == PowerBroadcastStatus.ResumeSuspend && _vpnConnectionStateMachine.IsConnected)
        {
            _logger.Info<ConnectionLog>("Resetting connection due to resume from sleep.");
            _vpnConnectionStateMachine.Reconnect();
        }

        return true;
    }

    private void OnPowerEventResume(object? sender, EventArgs e)
    {
        _logger.Info<OperatingSystemLog>($"{nameof(OnPowerEventResume)}");
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

    private bool IsBfeServiceRunningAndEnabled()
    {
        string bfeServiceName = _staticConfig.BaseFilteringEngineServiceName;

        try
        {
            using ServiceController serviceController = new(bfeServiceName);
            ServiceStartMode bfeServiceStartType = serviceController.StartType;
            ServiceControllerStatus bfeServiceStatus = serviceController.Status;

            _logger.Info<AppServiceStartLog>($"{bfeServiceName} Service - Start type: {bfeServiceStartType}, Status: {bfeServiceStatus}");

            return bfeServiceStartType != ServiceStartMode.Disabled
                && bfeServiceStatus == ServiceControllerStatus.Running;
        }
        catch (Exception e)
        {
            string errorMessage = $"Error checking BFE service status. Service name: {bfeServiceName}.";
            _logger.Error<AppServiceStartLog>(errorMessage, e);
            _issueReporter.CaptureError(errorMessage, e.Message);
            return true;
        }
    }
}