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

using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.AdvApi32;
using static Vanara.PInvoke.PowrProf;
using static Vanara.PInvoke.User32;

namespace ProtonVPN.OperatingSystems.PowerEvents;

public static class SystemPowerNotifications
{
    private static SystemPowerNotificationEventHandler _powerModeChanged;
    private static SafeHPOWERNOTIFY _powerEventHandler;
    private static SafeHPOWERSETTINGNOTIFY _displayEventHandler;
    private static SERVICE_STATUS_HANDLE _ssh;
    private static DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS _dnsp = new()
    {
        Callback = OnDeviceNotify,
        Context = IntPtr.Zero
    };

    public static string ServiceName { get; set; }

    public static event SystemPowerNotificationEventHandler PowerModeChanged
    {
        add
        {
            _powerModeChanged += value;
            RegisterPowerSuspendResumeNotification();
        }
        remove
        {
            _powerModeChanged -= value;
            DeregisterPowerSuspendResumeNotification();
        }
    }

    private static void RegisterPowerSuspendResumeNotification()
    {
        if (_powerEventHandler == null)
        {
            if (!string.IsNullOrEmpty(ServiceName))
            {
                if (_ssh.IsNull)
                {
                    _ssh = RegisterServiceCtrlHandlerEx(ServiceName, OnDisplayNotify);
                }

                if (_ssh.IsNull)
                {
                    throw new Exception("Failed To Register ServiceCtrlHandlerEx");
                }

                _displayEventHandler = RegisterPowerSettingNotification(((IntPtr)_ssh), GUID_MONITOR_POWER_ON, User32.DEVICE_NOTIFY.DEVICE_NOTIFY_SERVICE_HANDLE);
                if (_displayEventHandler.IsNull)
                {
                    throw new Exception("Failed To Register PowerSettingNotification");
                }
            }

            Win32Error result = PowerRegisterSuspendResumeNotification(RegisterSuspendResumeNotificationFlags.DEVICE_NOTIFY_CALLBACK,
            _dnsp, out _powerEventHandler);
            if (result != Win32Error.ERROR_SUCCESS)
            {
                throw new Exception("Failed To Register PowerSuspendResumeNotification");
            }
        }
    }

    private static void DeregisterPowerSuspendResumeNotification()
    {
        if (_powerModeChanged == null)
        {
            if (!string.IsNullOrEmpty(ServiceName))
            {
                if (!UnregisterPowerSettingNotification(_displayEventHandler))
                {
                    throw new Exception("Failed To Unregister PowerSettingNotification");
                }

                _displayEventHandler.Dispose();
                _displayEventHandler = null;
            }

            if (PowerUnregisterSuspendResumeNotification(_powerEventHandler) != Win32Error.NO_ERROR)
            {
                throw new Exception("Failed To Unregister PowerSuspendResumeNotification");
            }

            _powerEventHandler.Dispose();
            _powerEventHandler = null;
        }
    }

    private static Win32Error OnDeviceNotify(IntPtr context, uint type, IntPtr setting)
    {
        _powerModeChanged?.Invoke(null, new PowerNotificationArgs((PowerBroadcastType)type));
        return 0;
    }

    private static Win32Error OnDisplayNotify(ServiceControl control, uint eventType, IntPtr eventData, IntPtr context)
    {
        HANDLE dataHandle = new(eventData);
        HANDLE contextHandle = new(context);
        if (control == ServiceControl.SERVICE_CONTROL_POWEREVENT)
        {
            PowerBroadcastSetting settings = (PowerBroadcastSetting)Marshal.PtrToStructure(eventData, typeof(PowerBroadcastSetting));
            _powerModeChanged?.Invoke(null, new PowerNotificationArgs((PowerBroadcastType)eventType, settings.Data));
        }

        return 0;
    }
}