#pragma once
#include "pch.h"
#include "msiquery.h"
#include "Service.h"
#include <iostream>
#include <string>
#include "Utils.h"

#define EXPORT __declspec(dllexport)

const auto ServiceNameProperty = L"ServiceName";
const auto UpdateServiceNameProperty = L"UpdateServiceName";
const auto SplitTunnelServiceNameProperty = L"SplitTunnelServiceName";
const auto SplitTunnelServiceDisplayNameProperty = L"SplitTunnelServiceDisplayName";
const auto SplitTunnelDriverFileProperty = L"SplitTunnelDriverFile";

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

extern "C" EXPORT long UninstallSplitTunnelDriver(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    const auto serviceName = GetProperty(SplitTunnelServiceNameProperty);

    const auto result = DeleteService(serviceName);
    LogMessage(L"DeleteService returned: ", result);

    return result;
}

extern "C" EXPORT long InstallSplitTunnelDriver(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    const auto driverFile = GetProperty(SplitTunnelDriverFileProperty);
    const auto serviceName = GetProperty(SplitTunnelServiceNameProperty);
    const auto serviceDisplayName = GetProperty(SplitTunnelServiceDisplayNameProperty);

    const auto result = CreateDriverService(
        serviceName,
        serviceDisplayName,
        driverFile
    );
    LogMessage(L"CreateDriverService returned: ", result);

    return result;
}
