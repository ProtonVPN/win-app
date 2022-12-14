#pragma once
#include "pch.h"
#include <filesystem>
#include <fstream>
#include <string>
#include <strsafe.h>
#include "EnvironmentVariable.h"
#include "Service.h"
#include "msiquery.h"
#include "StartupApp.h"
#include "SystemState.h"
#include "Utils.h"
#include "bitcompressor.hpp"
#include "bitexception.hpp"
#include "PathManager.h"
#include "StringConverter.h"
#include "Installer.h"
#include "TunAdapter.h"
#include "ip_filter.h"

#define EXPORT __declspec(dllexport)

using namespace std;

const auto ServiceNameProperty = L"ServiceName";
const auto CalloutServiceNameProperty = L"CalloutServiceName";
const auto CalloutServiceDisplayNameProperty = L"CalloutServiceDisplayName";
const auto CalloutDriverFileProperty = L"CalloutDriverFile";
const auto SplitTunnelServiceNameProperty = L"SplitTunnelServiceName";
const auto SplitTunnelServiceDisplayNameProperty = L"SplitTunnelServiceDisplayName";
const auto SplitTunnelDriverFileProperty = L"SplitTunnelDriverFile";
const auto ProductLanguageProperty = L"ProductLanguage";
const auto SelectedLanguageProperty = L"SelectedLanguage";
const auto MsiLogFileLocationProperty = L"MsiLogFileLocation";
const auto LocalAppDataFolderProperty = L"LocalAppDataFolder";
const auto ApplicationDirectoryProperty = L"APPDIR";
const auto WintunDllPathProperty = L"WintunDllPath";
const auto ProductUpgradeCodeProperty = L"ProductUpgradeCode";
const auto RebootRequiredProperty = L"RebootRequired";
const auto ProductNameProperty = L"ProductName";

GUID providerGUID = { 0x20865f68, 0x0b04, 0x44da, { 0xbb, 0x83, 0x22, 0x38, 0x62, 0x25, 0x40, 0xfa } };
GUID sublayerGUID = { 0xaa867e71, 0x5765, 0x4be3, { 0x93, 0x99, 0x58, 0x15, 0x85, 0xc2, 0x26, 0xce } };

extern "C" EXPORT long RemoveWfpObjects(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);

    IPFilterSessionHandle h = nullptr;
    UINT status = IPFilterCreateSession(&h);
    if (status != NO_ERROR)
    {
        return status;
    }

    IPFilterStartTransaction(h);

    status = IPFilterDestroySublayerFilters(h, &providerGUID, &sublayerGUID);
    if (status != NO_ERROR)
    {
        goto abort_transaction;
    }

    status = IPFilterDestroyCallouts(h, &providerGUID);
    if (status != NO_ERROR)
    {
        goto abort_transaction;
    }

    status = IPFilterDestroySublayer(h, &sublayerGUID);
    if (status != NO_ERROR)
    {
        goto abort_transaction;
    }

    status = IPFilterDestroyProvider(h, &providerGUID);
    IPFilterCommitTransaction(h);
    goto close_session;

abort_transaction:
    IPFilterAbortTransaction(h);
close_session:
    IPFilterDestroySession(h);

    return status;
}

extern "C" EXPORT long UninstallProduct(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    return Uninstall(GetProperty(ProductUpgradeCodeProperty).c_str());
}

extern "C" EXPORT long InstallTunAdapter(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    return InstallTunAdapter(GetProperty(WintunDllPathProperty).c_str());
}

extern "C" EXPORT long UninstallTunAdapter(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    BOOL* rebootRequired = nullptr;
    const auto result = UninstallTunAdapter(GetProperty(WintunDllPathProperty).c_str(), rebootRequired);
    if (rebootRequired)
    {
        MsiSetProperty(hInstall, RebootRequiredProperty, L"1");
    }

    return result;
}

extern "C" EXPORT long ModifyServicePermissions(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);
    auto serviceName = GetProperty(ServiceNameProperty);
    auto result = ModifyServicePermissions(serviceName);
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

extern "C" EXPORT long RemoveProgramData(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);

    const string programDataPath = GetEnvironmentVariable("PROGRAMDATA");
    if (programDataPath.empty())
    {
        return -1;
    }
    const std::filesystem::path path(programDataPath + "\\ProtonVPN");
    std::filesystem::remove_all(path);

    return 0;
}

extern "C" EXPORT long SetLanguageISOCode(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);

    const wstring strValue = GetProperty(ProductLanguageProperty);
    const int value = std::stoi(strValue);
    wstring language = L"";
    switch (value)
    {
        case 1031: language = L"de-DE"; break;
        case 1033: language = L"en-US"; break;
        case 1034: language = L"es-ES"; break;
        case 58378: language = L"es-419"; break;
        case 1065: language = L"fa-IR"; break;
        case 1036: language = L"fr-FR"; break;
        case 1040: language = L"it-IT"; break;
        case 1043: language = L"nl-NL"; break;
        case 1045: language = L"pl-PL"; break;
        case 1046: language = L"pt-BR"; break;
        case 2070: language = L"pt-PT"; break;
        case 1049: language = L"ru-RU"; break;
        case 1055: language = L"tr-TR"; break;
        case 1050: language = L"hr-HR"; break;
        case 1057: language = L"id-ID"; break;
        case 1058: language = L"uk-UA"; break;
        case 1029: language = L"cs-CZ"; break;
        case 1048: language = L"ro-RO"; break;
    }
    if (!language.empty())
    {
        SetProperty(SelectedLanguageProperty, language);
    }

    return 0;
}

void AddFileToZip(const std::wstring& zipFileName, const std::wstring& fileName)
{
    std::wstring installDirectory = GetProperty(ApplicationDirectoryProperty);
    const std::wstring sevenZipDllPath = AddEndingSlashIfNotExists(installDirectory) + L"7za.dll";

    try
    {
        bit7z::Bit7zLibrary lib{ sevenZipDllPath };
        bit7z::BitCompressor compressor{ lib, bit7z::BitFormat::SevenZip };

        std::vector<std::wstring> files = { fileName };

        compressor.setUpdateMode(true);
        compressor.compressFiles(files, zipFileName);
    }
    catch (const bit7z::BitException& ex)
    {
        LogMessage(L"Error when compressing file to zip: " + Utf8StringToWstring(ex.what()));
    }
}

extern "C" EXPORT long CopyInstallLog(MSIHANDLE hInstall)
{
    SetMsiHandle(hInstall);

    const std::wstring productName = GetProperty(ProductNameProperty);
    const std::wstring logFile = GetProperty(MsiLogFileLocationProperty);
    std::wstring localAppDataFolder = GetProperty(LocalAppDataFolderProperty);
    const std::wstring logFolder = AddEndingSlashIfNotExists(localAppDataFolder) + L"ProtonVPN\\DiagnosticLogs\\";
    const std::wstring tmpLogFile = logFolder + productName + L"_install.log";

    std::filesystem::create_directories(logFolder);
    const std::filesystem::copy_options copyOptions = std::filesystem::copy_options::overwrite_existing;
    copy_file(logFile, tmpLogFile, copyOptions);

    const std::wstring zipFileName = logFolder + productName + L"_install-log.7z";

    AddFileToZip(zipFileName, tmpLogFile);
    std::filesystem::remove(tmpLogFile);

    return 0;
}