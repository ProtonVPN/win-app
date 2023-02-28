#include "ServiceManager.h"
#include <iostream>
#include <sddl.h>
#include <minwindef.h>
#include "Utils.h"
#include "WinApiErrorException.h"

using namespace std;

void ServiceManager::Create(wstring name, wstring display_name, wstring file_path, DWORD type)
{
    wstring path;
    if (type == SERVICE_KERNEL_DRIVER)
    {
        path += L"\\??\\" + file_path;
    }
    else
    {
        path += L"\"" + file_path + L"\"";
    }

    wil::unique_schandle sc_manager = GetServiceManagerHandle();
    wil::unique_schandle service;

    if (Exists(name))
    {
        service = GetServiceHandle(sc_manager, name, SERVICE_ALL_ACCESS);
    }
    else
    {
        service = wil::unique_schandle(CreateService(
            sc_manager.get(),
            name.c_str(),
            display_name.c_str(),
            SERVICE_ALL_ACCESS,
            type,
            SERVICE_DEMAND_START,
            SERVICE_ERROR_NORMAL,
            (type == SERVICE_KERNEL_DRIVER ? path + L"\"" : path).c_str(),
            L"NDIS",
            nullptr,
            L"TCPIP\0",
            nullptr,
            nullptr
        ));
    }

    if (service == nullptr)
    {
        throw WinApiErrorException(L"Failed to create service " + name + L".", GetLastError());
    }

    const bool changed = ChangeServiceConfig(service.get(),
                                             SERVICE_NO_CHANGE,
                                             SERVICE_NO_CHANGE,
                                             SERVICE_NO_CHANGE,
                                             path.c_str(),
                                             nullptr,
                                             nullptr,
                                             nullptr,
                                             nullptr,
                                             nullptr,
                                             nullptr);
    if (!changed)
    {
        throw WinApiErrorException(L"Failed to change service " + name + L" config.", GetLastError());
    }

    if (type != SERVICE_KERNEL_DRIVER)
    {
        ModifyPermissions(service, name);
    }
}

void ServiceManager::Delete(wstring name)
{
    wil::unique_schandle sc_manager = GetServiceManagerHandle();
    wil::unique_schandle service = GetServiceHandle(sc_manager, name, SERVICE_ALL_ACCESS);
    if (service == nullptr)
    {
        const DWORD last_error = GetLastError();
        if (last_error == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            LogMessage(L"Service " + name + L" does not exist. Skipping its removal.");
            return;
        }

        throw WinApiErrorException(L"Failed to receive service handle on service " + name + L" delete.",
                                   GetLastError());
    }

    Stop(service, name);

    if (!DeleteService(service.get()))
    {
        throw WinApiErrorException(L"Failed to delete service " + name + L".", GetLastError());
    }
}

void ServiceManager::Stop(wil::unique_schandle& handle, wstring name)
{
    SERVICE_STATUS_PROCESS status = GetStatus(handle);
    if (status.dwCurrentState == SERVICE_STOPPED)
    {
        LogMessage(L"Service " + name + L" is already stopped.");
        return;
    }

    DWORD wait_time;
    DWORD start_time = GetTickCount();

    while (status.dwCurrentState == SERVICE_STOP_PENDING)
    {
        LogMessage(L"Service " + name + L" stop pending...");

        wait_time = GetWaitTime(status);
        Sleep(wait_time);

        status = GetStatus(handle);
        if (status.dwCurrentState == SERVICE_STOPPED)
        {
            LogMessage(L"Service " + name + L" stopped.");
            return;
        }

        if (GetTickCount() - start_time > timeout_)
        {
            LogMessage(L"Service " + name + L" stop timed out.");
        }
    }

    if (!ControlService(handle.get(), SERVICE_CONTROL_STOP, reinterpret_cast<LPSERVICE_STATUS>(&status)))
    {
        const DWORD last_error = GetLastError();
        if (last_error != ERROR_SERVICE_NOT_ACTIVE)
        {
            throw WinApiErrorException(L"Failed to stop service " + name + L".", GetLastError());
        }
    }

    status = GetStatus(handle);
    while (status.dwCurrentState != SERVICE_STOPPED)
    {
        Sleep(status.dwWaitHint);
        status = GetStatus(handle);

        if (status.dwCurrentState == SERVICE_STOPPED)
        {
            LogMessage(L"Service " + name + L" stopped.");
            break;
        }

        if (GetTickCount() - start_time > timeout_)
        {
            LogMessage(L"Service " + name + L" stop timed out.");
        }
    }
}

DWORD ServiceManager::GetWaitTime(SERVICE_STATUS_PROCESS status)
{
    DWORD wait_time = status.dwWaitHint / 10;
    if (wait_time < 1000)
    {
        wait_time = 1000;
    }
    else if (wait_time > 10000)
    {
        wait_time = 10000;
    }

    return wait_time;
}

SERVICE_STATUS_PROCESS ServiceManager::GetStatus(wil::unique_schandle& handle)
{
    SERVICE_STATUS_PROCESS status;
    DWORD bytes_needed;

    if (!QueryServiceStatusEx(
        handle.get(),
        SC_STATUS_PROCESS_INFO,
        reinterpret_cast<LPBYTE>(&status),
        sizeof(SERVICE_STATUS_PROCESS),
        &bytes_needed))
    {
        throw WinApiErrorException(L"QueryServiceStatusEx failed.", GetLastError());
    }

    return status;
}

void ServiceManager::ModifyPermissions(wil::unique_schandle& handle, wstring name)
{
    wil::unique_hlocal_security_descriptor security_descriptor;

    if (!ConvertStringSecurityDescriptorToSecurityDescriptor(
        L"D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)"
        L"(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)"
        L"(A;;CCLCSWLOCRRC;;;IU)"
        L"(A;;CCLCSWLOCRRC;;;SU)"
        L"(A;;RPWPLC;;;IU)"
        L"S:(AU;FA;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;WD)",
        SDDL_REVISION_1, &security_descriptor, nullptr))
    {
        throw WinApiErrorException(L"Failed to convert string security descriptor for service " + name + L".",
                                   GetLastError());
    }

    if (!SetServiceObjectSecurity(handle.get(), DACL_SECURITY_INFORMATION, security_descriptor.get()))
    {
        throw WinApiErrorException(L"Failed to set security descriptor for service " + name + L".", GetLastError());
    }
}

bool ServiceManager::Exists(std::wstring name)
{
    wil::unique_schandle handle = GetServiceManagerHandle();
    SC_HANDLE service = OpenService(handle.get(), name.c_str(), SERVICE_QUERY_CONFIG);
    if (service == nullptr)
    {
        DWORD last_error = GetLastError();
        if (last_error == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            return false;
        }

        throw WinApiErrorException(L"Failed to open service " + name + L".", last_error);
    }

    return true;
}

wil::unique_schandle ServiceManager::GetServiceManagerHandle()
{
    wil::unique_schandle handle = wil::unique_schandle(OpenSCManager(nullptr, nullptr, SC_MANAGER_ALL_ACCESS));
    if (handle == nullptr)
    {
        throw WinApiErrorException(L"Failed to open service manager.", GetLastError());
    }

    return handle;
}

wil::unique_schandle ServiceManager::GetServiceHandle(wil::unique_schandle& handle, wstring name, DWORD access)
{
    return wil::unique_schandle(OpenService(handle.get(), name.c_str(), access));
}
