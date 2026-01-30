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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Monitors;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.OperatingSystems.Network.Monitors;

public sealed class RouteChangeMonitor : IRouteChangeMonitor
{
    private readonly ILogger _logger;
    private readonly object _lock = new();
    private readonly PIPFORWARD_CHANGE_CALLBACK _callback;
    private IntPtr _notificationHandle;
    private bool _isRunning;

    public event EventHandler<RouteChangeEventArgs>? RouteChanged;

    public RouteChangeMonitor(ILogger logger)
    {
        _logger = logger;
        _callback = OnRouteChanged;
    }

    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                return;
            }

            try
            {
                Win32Error result = NotifyRouteChange2(
                    ADDRESS_FAMILY.AF_UNSPEC,
                    _callback,
                    IntPtr.Zero,
                    false,
                    out _notificationHandle);

                if (result.Failed)
                {
                    _notificationHandle = IntPtr.Zero;
                    _logger.Warn<NetworkLog>("Failed to subscribe to route change notifications.", result.GetException());
                    return;
                }

                _isRunning = true;
            }
            catch (Exception e)
            {
                _logger.Error<NetworkLog>("Exception thrown when subscribing to route change notifications.", e);
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                return;
            }

            if (_notificationHandle != IntPtr.Zero)
            {
                try
                {
                    CancelMibChangeNotify2(_notificationHandle);
                }
                catch (Exception e)
                {
                    _logger.Error<NetworkLog>("Failed to unsubscribe from route change notifications.", e);
                }

                _notificationHandle = IntPtr.Zero;
            }

            _isRunning = false;
        }
    }

    private void OnRouteChanged(IntPtr callerContext, ref MIB_IPFORWARD_ROW2 row, MIB_NOTIFICATION_TYPE notificationType)
    {
        if (notificationType == MIB_NOTIFICATION_TYPE.MibInitialNotification)
        {
            return;
        }

        try
        {
            RouteChanged?.Invoke(this, new RouteChangeEventArgs());
        }
        catch (Exception ex)
        {
            _logger.Warn<NetworkLog>("Failed to evaluate route change notification.", ex);
        }
    }
}