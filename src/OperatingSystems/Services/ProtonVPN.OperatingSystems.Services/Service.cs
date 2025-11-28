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

using System.ComponentModel;
using System.Runtime.InteropServices;
using Polly;
using Polly.Retry;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.OperatingSystems.Services.Contracts;
using static Vanara.PInvoke.AdvApi32;

namespace ProtonVPN.OperatingSystems.Services;

public class Service : IService
{
    private const int RETRY_COUNT = 2;
    private const int RETRY_DELAY_IN_SECONDS = 1;
    private const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
    private const int ERROR_SERVICE_NOT_ACTIVE = 1062;
    private const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const uint SERVICE_SID_TYPE_UNRESTRICTED = 0x00000001;

    private readonly TimeSpan _timeoutInterval = TimeSpan.FromSeconds(10);
    private readonly ILogger _logger;
    private readonly ICommandLineCaller _commandLineCaller;

    public string Name { get; }

    public Service(string name, ILogger logger, ICommandLineCaller commandLineCaller)
    {
        Name = name;
        _logger = logger;
        _commandLineCaller = commandLineCaller;
    }

    public bool IsCreated()
    {
        try
        {
            using SafeSC_HANDLE handle = GetServiceHandle(ServiceAccessTypes.SERVICE_QUERY_STATUS);
            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Unable to determine whether Windows service '{Name}' exists.", ex);
            return false;
        }
    }

    public bool IsEnabled()
    {
        try
        {
            using SafeSC_HANDLE handle = GetServiceHandle(ServiceAccessTypes.SERVICE_QUERY_CONFIG);
            QUERY_SERVICE_CONFIG config = GetServiceConfig(handle);
            return config.dwStartType != ServiceStartType.SERVICE_DISABLED;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            _logger.Error<OperatingSystemLog>($"The service '{Name}' does not exist.", ex);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Unable to determine whether Windows service '{Name}' is enabled.", ex);
            return false;
        }
    }

    private SafeSC_HANDLE GetServiceHandle(ServiceAccessTypes desiredAccess)
    {
        SafeSC_HANDLE scManagerHandle = OpenSCManager(null, null, ScManagerAccessTypes.SC_MANAGER_CONNECT);
        if (scManagerHandle.IsInvalid)
        {
            scManagerHandle.Dispose();
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to open the Service Control Manager to query '{Name}'.");
        }

        try
        {
            SafeSC_HANDLE serviceHandle = OpenService(scManagerHandle, Name, desiredAccess);
            if (serviceHandle.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                serviceHandle.Dispose();

                throw new Win32Exception(errorCode, $"Failed to open Windows service '{Name}'.");
            }

            return serviceHandle;
        }
        finally
        {
            scManagerHandle.Dispose();
        }
    }

    private QUERY_SERVICE_CONFIG GetServiceConfig(SafeSC_HANDLE serviceHandle)
    {
        if (!QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out uint bytesNeeded))
        {
            int queryError = Marshal.GetLastWin32Error();
            if (queryError != ERROR_INSUFFICIENT_BUFFER)
            {
                throw new Win32Exception(queryError, $"Failed to query configuration size for service '{Name}'.");
            }
        }

        if (bytesNeeded == 0)
        {
            throw new Win32Exception($"Service '{Name}' returned no configuration data.");
        }

        IntPtr buffer = Marshal.AllocHGlobal((int)bytesNeeded);
        try
        {
            if (!QueryServiceConfig(serviceHandle, buffer, bytesNeeded, out _))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to query configuration for service '{Name}'.");
            }

            return Marshal.PtrToStructure<QUERY_SERVICE_CONFIG>(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Enable()
    {
        _logger.Info<OperatingSystemLog>($"Enabling the Windows service '{Name}'.");
        _commandLineCaller.ExecuteElevated($"/c sc config \"{Name}\" start= demand");
    }

    public void UpdatePathAndArgs(string pathAndArgs)
    {
        try
        {
            using SafeSC_HANDLE scManagerHandle = OpenSCManager(null, null, ScManagerAccessTypes.SC_MANAGER_CONNECT);
            if (scManagerHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            using SafeSC_HANDLE serviceHandle = OpenService(scManagerHandle, Name, ServiceAccessTypes.SERVICE_CHANGE_CONFIG);
            if (serviceHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!ChangeServiceConfig(serviceHandle,
                    ServiceTypes.SERVICE_NO_CHANGE,
                    ServiceStartType.SERVICE_NO_CHANGE,
                    ServiceErrorControlType.SERVICE_NO_CHANGE,
                    pathAndArgs,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null,
                    null))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to update Windows service '{Name}' path.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Failed to update Windows service '{Name}' path.", ex);
        }
    }

    public string? GetBinaryPath()
    {
        try
        {
            using SafeSC_HANDLE handle = GetServiceHandle(ServiceAccessTypes.SERVICE_QUERY_CONFIG);
            QUERY_SERVICE_CONFIG config = GetServiceConfig(handle);
            return config.lpBinaryPathName;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            _logger.Error<OperatingSystemLog>($"The service '{Name}' does not exist.", ex);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Failed to retrieve Windows service '{Name}' binary path.", ex);
            return null;
        }
    }

    public bool IsRunning()
    {
        return GetServiceState() == ServiceState.SERVICE_RUNNING;
    }

    public bool IsStopped()
    {
        ServiceState? state = GetServiceState();
        return !state.HasValue || state.Value == ServiceState.SERVICE_STOPPED;
    }

    public bool Start()
    {
        return StartInternalAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public bool StartWithRetry()
    {
        return GetRetryPolicy().Execute(() => StartInternalAsync(CancellationToken.None).GetAwaiter().GetResult());
    }

    private static RetryPolicy<bool> GetRetryPolicy()
    {
        return Policy<bool>
            .HandleResult(result => false)
            .WaitAndRetry(RETRY_COUNT, attempt => TimeSpan.FromSeconds(RETRY_DELAY_IN_SECONDS));
    }

    public Task<bool> StartAsync(CancellationToken cancellationToken)
    {
        return StartInternalAsync(cancellationToken);
    }

    private async Task<bool> StartInternalAsync(CancellationToken cancellationToken)
    {
        _logger.Info<OperatingSystemLog>($"Starting the Windows service '{Name}'.");

        try
        {
            using SafeSC_HANDLE handle = GetServiceHandle(ServiceAccessTypes.SERVICE_START | ServiceAccessTypes.SERVICE_QUERY_STATUS);
            if (!StartService(handle))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != ERROR_SERVICE_ALREADY_RUNNING)
                {
                    throw new Win32Exception(errorCode, $"Failed to start Windows service '{Name}'.");
                }
            }

            if (!await WaitForServiceStateAsync(handle, ServiceState.SERVICE_RUNNING, cancellationToken).ConfigureAwait(false))
            {
                _logger.Warn<OperatingSystemLog>($"The service '{Name}' did not reach the running state within {_timeoutInterval.TotalSeconds} seconds.");
                return false;
            }

            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            _logger.Error<OperatingSystemLog>($"The service '{Name}' does not exist.", ex);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Failed to start Windows service '{Name}'.", ex);
            return false;
        }
    }

    public bool Stop()
    {
        return StopInternalAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public bool StopWithRetry()
    {
        return GetRetryPolicy().Execute(() => StopInternalAsync(CancellationToken.None).GetAwaiter().GetResult());
    }

    public Task<bool> StopAsync(CancellationToken cancellationToken)
    {
        return StopInternalAsync(cancellationToken);
    }

    private async Task<bool> StopInternalAsync(CancellationToken cancellationToken)
    {
        _logger.Info<OperatingSystemLog>($"Stopping the Windows service '{Name}'.");

        try
        {
            using SafeSC_HANDLE handle = GetServiceHandle(ServiceAccessTypes.SERVICE_STOP | ServiceAccessTypes.SERVICE_QUERY_STATUS);

            if (!ControlService(handle, ServiceControl.SERVICE_CONTROL_STOP, out SERVICE_STATUS status))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == ERROR_SERVICE_NOT_ACTIVE)
                {
                    return true;
                }

                throw new Win32Exception(errorCode, $"Failed to send stop command to Windows service '{Name}'.");
            }

            if (!await WaitForServiceStateAsync(handle, ServiceState.SERVICE_STOPPED, cancellationToken).ConfigureAwait(false))
            {
                _logger.Warn<OperatingSystemLog>($"The service '{Name}' did not reach the stopped state within {_timeoutInterval.TotalSeconds} seconds.");
                return false;
            }

            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            _logger.Error<OperatingSystemLog>($"The service '{Name}' does not exist.", ex);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Failed to stop Windows service '{Name}'.", ex);
            return false;
        }
    }

    private async Task<bool> WaitForServiceStateAsync(SafeSC_HANDLE serviceHandle, ServiceState desiredState, CancellationToken cancellationToken)
    {
        DateTime start = DateTime.UtcNow;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!QueryServiceStatus(serviceHandle, out SERVICE_STATUS status))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to query status for service '{Name}'.");
            }

            if (status.dwCurrentState == desiredState)
            {
                return true;
            }

            if (status.dwCurrentState == ServiceState.SERVICE_STOPPED && desiredState == ServiceState.SERVICE_RUNNING)
            {
                return false;
            }

            if (DateTime.UtcNow - start >= _timeoutInterval)
            {
                return false;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }
    }

    public ServiceStatus? GetStatus()
    {
        ServiceState? state = GetServiceState();

        return state.HasValue
            ? MapServiceState(state.Value)
            : null;
    }

    public void Create(ServiceCreationOptions options)
    {
        try
        {
            using SafeSC_HANDLE scManagerHandle = OpenSCManager(dwDesiredAccess: ScManagerAccessTypes.SC_MANAGER_CREATE_SERVICE);
            if (scManagerHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            string[]? dependencies = options.Dependencies.Count > 0 ? [.. options.Dependencies] : null;

            using SafeSC_HANDLE serviceHandle = CreateService(
                scManagerHandle,
                Name,
                Name,
                (uint)ServiceAccessRights.SERVICE_ALL_ACCESS,
                ServiceTypes.SERVICE_WIN32_OWN_PROCESS,
                ServiceStartType.SERVICE_DEMAND_START,
                ServiceErrorControlType.SERVICE_ERROR_NORMAL,
                options.PathAndArguments,
                null,
                IntPtr.Zero,
                dependencies);

            if (serviceHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to create Windows service '{Name}'.");
            }

            if (options.IsUnrestricted)
            {
                SERVICE_SID_INFO sidInfo = new()
                {
                    dwServiceSidType = SERVICE_SID_TYPE_UNRESTRICTED,
                };

                if (!ChangeServiceConfig2(serviceHandle, ServiceConfigOption.SERVICE_CONFIG_SERVICE_SID_INFO, sidInfo))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to set SID type for service '{Name}'.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Failed to create Windows service '{Name}'.", ex);
        }
    }

    private ServiceState? GetServiceState()
    {
        try
        {
            using SafeSC_HANDLE handle = GetServiceHandle(ServiceAccessTypes.SERVICE_QUERY_STATUS);
            if (!QueryServiceStatus(handle, out SERVICE_STATUS status))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to query status for service '{Name}'.");
            }

            return status.dwCurrentState;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            _logger.Error<OperatingSystemLog>($"The service '{Name}' does not exist.", ex);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error<OperatingSystemLog>($"Unable to query Windows service '{Name}' status.", ex);
            return null;
        }
    }

    private static ServiceStatus MapServiceState(ServiceState state)
    {
        return state switch
        {
            ServiceState.SERVICE_STOPPED => ServiceStatus.Stopped,
            ServiceState.SERVICE_START_PENDING => ServiceStatus.StartPending,
            ServiceState.SERVICE_STOP_PENDING => ServiceStatus.StopPending,
            ServiceState.SERVICE_RUNNING => ServiceStatus.Running,
            ServiceState.SERVICE_CONTINUE_PENDING => ServiceStatus.ContinuePending,
            ServiceState.SERVICE_PAUSE_PENDING => ServiceStatus.PausePending,
            ServiceState.SERVICE_PAUSED => ServiceStatus.Paused,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown service state.")
        };
    }
}