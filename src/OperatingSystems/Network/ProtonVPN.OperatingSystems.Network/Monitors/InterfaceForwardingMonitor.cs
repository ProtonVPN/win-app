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

using System.Runtime.InteropServices;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.Monitors;
using Vanara.PInvoke;
using static Vanara.PInvoke.IpHlpApi;
using static Vanara.PInvoke.Ws2_32;

namespace ProtonVPN.OperatingSystems.Network.Monitors;

public sealed class InterfaceForwardingMonitor : IInterfaceForwardingMonitor
{
    private readonly ILogger _logger;
    private readonly object _lock = new();
    private readonly PIPINTERFACE_CHANGE_CALLBACK _callback;
    private IntPtr _notificationHandle;
    private bool _isRunning;

    public event EventHandler<InterfaceForwardingEventArgs>? ForwardingEnabled;

    public InterfaceForwardingMonitor(ILogger logger)
    {
        _logger = logger;
        _callback = OnIpInterfaceChanged;
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
                Win32Error result = NotifyIpInterfaceChange(
                    ADDRESS_FAMILY.AF_UNSPEC,
                    _callback,
                    CallerContext: IntPtr.Zero,
                    InitialNotification: false,
                    NotificationHandle: out _notificationHandle);

                if (result.Failed)
                {
                    _notificationHandle = IntPtr.Zero;
                    _logger.Warn<NetworkLog>("Failed to subscribe to IP interface change notifications.", result.GetException());
                    return;
                }

                _isRunning = true;
            }
            catch (Exception e)
            {
                _logger.Error<NetworkLog>("Failed to subscribe to a network interface changes.", e);
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
                    _logger.Error<NetworkLog>("Failed to unsubscribe from a network interface changes.", e);
                }

                _notificationHandle = IntPtr.Zero;
            }

            _isRunning = false;
        }
    }

    private void OnIpInterfaceChanged(IntPtr callerContext, IntPtr row, MIB_NOTIFICATION_TYPE notificationType)
    {
        if (row == IntPtr.Zero || notificationType != MIB_NOTIFICATION_TYPE.MibParameterNotification)
        {
            return;
        }

        try
        {
            MIB_IPINTERFACE_ROW interfaceRow = Marshal.PtrToStructure<MIB_IPINTERFACE_ROW>(row);
            Win32Error refreshResult = GetIpInterfaceEntry(ref interfaceRow);
            if (refreshResult.Failed)
            {
                // Can be safely ignored as it happens only if the user tries to exit the app right after disconnecting,
                // and Stop() method is not yet called.
                if (refreshResult != Win32Error.ERROR_NOT_FOUND)
                {
                    _logger.Warn<NetworkLog>(
                        $"Failed to refresh interface information for index {interfaceRow.InterfaceIndex}.",
                        refreshResult.GetException());
                }
                return;
            }

            if (!interfaceRow.ForwardingEnabled)
            {
                return;
            }

            ForwardingEnabled?.Invoke(this, new InterfaceForwardingEventArgs(interfaceRow.InterfaceIndex));
        }
        catch (Exception ex)
        {
            _logger.Warn<NetworkLog>("Failed to evaluate IP interface change notification.", ex);
        }
    }
}