#include "pch.h"
#include <iostream>
#include <sddl.h>
#include <wil/resource.h>

int ModifyServicePermissions(std::wstring serviceName)
{
    const wil::unique_schandle scManager(OpenSCManager(nullptr, nullptr, 0));
    if (scManager == nullptr)
    {
        return GetLastError();
    }

    const wil::unique_schandle hService(OpenService(scManager.get(), serviceName.c_str(), WRITE_DAC));
    if (hService == nullptr)
    {
        return GetLastError();
    }

    wil::unique_hlocal_security_descriptor SecurityDescriptor;
    if (!ConvertStringSecurityDescriptorToSecurityDescriptorW(
        L"D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)"
        L"(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)"
        L"(A;;CCLCSWLOCRRC;;;IU)"
        L"(A;;CCLCSWLOCRRC;;;SU)"
        L"(A;;RPWPLC;;;IU)"
        L"S:(AU;FA;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;WD)",
        SDDL_REVISION_1, &SecurityDescriptor, 0))
    {
        return GetLastError();
    }

    if (!SetServiceObjectSecurity(hService.get(), DACL_SECURITY_INFORMATION, SecurityDescriptor.get()))
    {
        return GetLastError();
    }

    return 0;
}

int DeleteService(std::wstring serviceName)
{
    const wil::unique_schandle scManager(OpenSCManager(nullptr, nullptr, SC_MANAGER_ALL_ACCESS));
    if (scManager == nullptr)
    {
        return GetLastError();
    }

    const wil::unique_schandle service(OpenService(scManager.get(), serviceName.c_str(), GENERIC_ALL));
    if (service == nullptr)
    {
        const auto err = GetLastError();
        if (err == ERROR_SERVICE_DOES_NOT_EXIST)
        {
            return 0;
        }

        return err;
    }

    SERVICE_STATUS_PROCESS status;
    if (!ControlService(service.get(), SERVICE_CONTROL_STOP, reinterpret_cast<LPSERVICE_STATUS>(&status)))
    {
        const auto err = GetLastError();
        if (err != ERROR_SERVICE_NOT_ACTIVE)
        {
            return err;
        }
    }

    if (!DeleteService(service.get()))
    {
        return GetLastError();
    }

    return 0;
}

int CreateDriverService(std::wstring serviceName, std::wstring displayName, std::wstring driverFile)
{
    const auto absolutePath = L"\"\\??\\" + driverFile + L"\"";
    const auto kernelPath = L"\\??\\" + driverFile;

    const wil::unique_schandle scManager(OpenSCManager(nullptr, nullptr, SC_MANAGER_ALL_ACCESS));
    if (scManager == nullptr)
    {
        return GetLastError();
    }

    const wil::unique_schandle service(CreateService(
        scManager.get(),
        serviceName.c_str(),
        displayName.c_str(),
        SERVICE_ALL_ACCESS,
        SERVICE_KERNEL_DRIVER,
        SERVICE_DEMAND_START,
        SERVICE_ERROR_NORMAL,
        absolutePath.c_str(),
        L"NDIS",
        nullptr,
        L"TCPIP\0",
        nullptr,
        nullptr
    ));
    if (service == nullptr)
    {
        return GetLastError();
    }

    BOOL changed = ChangeServiceConfig(service.get(),
                                       SERVICE_NO_CHANGE,
                                       SERVICE_NO_CHANGE,
                                       SERVICE_NO_CHANGE,
                                       kernelPath.c_str(),
                                       nullptr,
                                       nullptr,
                                       nullptr,
                                       nullptr,
                                       nullptr,
                                       nullptr);
    if (!changed)
    {
        return GetLastError();
    }

    return 0;
}
