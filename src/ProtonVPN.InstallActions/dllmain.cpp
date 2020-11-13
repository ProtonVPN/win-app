#pragma once
#include "pch.h"
#include "msiquery.h"
#include "Service.h"
#include <iostream>
#include <string>
#include <strsafe.h>

#include "StartupApp.h"
#include "SystemState.h"
#include "Utils.h"

#define EXPORT __declspec(dllexport)

using namespace std;

const auto ServiceNameProperty = L"ServiceName";
const auto UpdateServiceNameProperty = L"UpdateServiceName";
const auto CalloutServiceNameProperty = L"CalloutServiceName";
const auto CalloutServiceDisplayNameProperty = L"CalloutServiceDisplayName";
const auto CalloutDriverFileProperty = L"CalloutDriverFile";

extern "C" EXPORT long ModifyServicePermissions(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    auto serviceName = GetProperty(ServiceNameProperty);
    auto result = ModifyServicePermissions(serviceName);
    LogMessage(L"ModifyServicePermissions returned: ", result);

    serviceName = GetProperty(UpdateServiceNameProperty);
    result = ModifyServicePermissions(serviceName);
    LogMessage(L"ModifyServicePermissions returned: ", result);

    return result;
}

extern "C" EXPORT long UninstallCalloutDriver(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    const auto serviceName = GetProperty(CalloutServiceNameProperty);

    const auto result = DeleteService(serviceName);
    LogMessage(L"DeleteService returned: ", result);

    return result;
}

extern "C" EXPORT long InstallCalloutDriver(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    const auto driverFile = GetProperty(CalloutDriverFileProperty);
    const auto serviceName = GetProperty(CalloutServiceNameProperty);
    const auto serviceDisplayName = GetProperty(CalloutServiceDisplayNameProperty);

    const auto result = CreateDriverService(
        serviceName,
        serviceDisplayName,
        driverFile
    );
    LogMessage(L"CreateDriverService returned: ", result);

    return result;
}

extern "C" EXPORT long PendingReboot(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    const auto result = pending_reboot();
	if (result)
	{
        SetProperty(L"PENDING_REBOOT", L"1");
	}

    LogMessage(L"Pending reboot value is: ", result);

    return 0;
}

extern "C" EXPORT long Reboot(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);

    HANDLE h_token = nullptr;
    TOKEN_PRIVILEGES tkp;

    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &h_token))
    {
        return 0;
    }

    LookupPrivilegeValue(nullptr, SE_SHUTDOWN_NAME, &tkp.Privileges[0].Luid);
    tkp.PrivilegeCount = 1;
    tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    AdjustTokenPrivileges(h_token, FALSE, &tkp, 0, static_cast<PTOKEN_PRIVILEGES>(nullptr), nullptr);
    if (GetLastError() == ERROR_SUCCESS)
    {
        add_installer_to_startup();
        ExitWindowsEx(EWX_REBOOT, SHTDN_REASON_MINOR_INSTALLATION);

        return 1;
    }

    return 0;
}
