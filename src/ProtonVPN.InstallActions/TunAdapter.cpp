#include "pch.h"
#include <winsock2.h>
#include <Windows.h>
#include "Utils.h"
#include "Wintun.h"

static WINTUN_CREATE_ADAPTER_FUNC WintunCreateAdapter;
static WINTUN_FREE_ADAPTER_FUNC WintunFreeAdapter;
static WINTUN_OPEN_ADAPTER_FUNC WintunOpenAdapter;
static WINTUN_DELETE_POOL_DRIVER_FUNC WintunDeletePoolDriver;
static WINTUN_SET_LOGGER_FUNC WintunSetLogger;

const WCHAR *AdapterPoolName = L"ProtonVPN";
const WCHAR *AdapterName = L"ProtonVPN TUN";
const GUID AdapterGuid = { 0xafdeecba, 0xdfba, 0xcaff, {0x50, 0x44, 0x01, 0x34, 0x12, 0xbc, 0xea, 0xcd} };

static void CALLBACK InstallLogger(_In_ WINTUN_LOGGER_LEVEL Level, _In_z_ const WCHAR* LogLine)
{
    LogMessage(LogLine);
}

static void SetCurrentDir(const std::wstring path, size_t lastSlashPosition)
{
    std::wstring directory = path.substr(0, lastSlashPosition);
    std::wstring newCurrentDirectory = directory + L"\\";
    LogMessage(L"Setting current directory to: " + newCurrentDirectory);
    if (!SetCurrentDirectory(newCurrentDirectory.c_str()))
    {
        LogMessage(L"Failed to change current directory: ", GetLastError());
    }
}

static HMODULE InitializeWintun(LPCWSTR dllPath)
{
    TCHAR currentDirectory[MAX_PATH];
    GetCurrentDirectory(MAX_PATH, currentDirectory);

    std::wstring pathStr = static_cast<std::wstring>(dllPath);
    const size_t lastSlashPosition = pathStr.rfind('\\');
    if (std::string::npos != lastSlashPosition)
    {
        SetCurrentDir(pathStr, lastSlashPosition);
    }

    std::wstring wintunDll = pathStr.substr(lastSlashPosition + 1, pathStr.length());
    HMODULE Wintun = LoadLibrary(wintunDll.c_str());
    SetCurrentDirectory(currentDirectory);

    if (!Wintun)
    {
        return nullptr;
    }

#define X(Name, Type) (((Name) = (Type)GetProcAddress(Wintun, #Name)) == NULL)
    if (X(WintunCreateAdapter, WINTUN_CREATE_ADAPTER_FUNC) ||
        X(WintunDeletePoolDriver, WINTUN_DELETE_POOL_DRIVER_FUNC) ||
        X(WintunFreeAdapter, WINTUN_FREE_ADAPTER_FUNC) ||
        X(WintunSetLogger, WINTUN_SET_LOGGER_FUNC) ||
        X(WintunOpenAdapter, WINTUN_OPEN_ADAPTER_FUNC))
#undef X
    {
        DWORD LastError = GetLastError();
        FreeLibrary(Wintun);
        SetLastError(LastError);

        return nullptr;
    }

    return Wintun;
}

int InstallTunAdapter(LPCWSTR dllPath)
{
    DWORD LastError = ERROR_SUCCESS;
    HMODULE Wintun = InitializeWintun(dllPath);

    if (!Wintun)
    {
        LastError = GetLastError();
        LogMessage(L"Failed to initialize Wintun: ", LastError);
        return LastError;
    }

    WintunSetLogger(InstallLogger);

    WINTUN_ADAPTER_HANDLE Adapter = WintunOpenAdapter(AdapterPoolName, AdapterName);
    if (Adapter != nullptr)
    {
        LogMessage(L"ProtonVPN TUN adapter detected. Trying to remove...");
        WintunFreeAdapter(Adapter);

        BOOL* rebootRequired = nullptr;
        if (WintunDeletePoolDriver(AdapterPoolName, rebootRequired) == 0)
        {
            LastError = GetLastError();
            LogMessage(L"Failed to uninstall ProtonVPN TUN adapter: ", LastError);
        }
        else
        {
            LogMessage(L"ProtonVPN TUN removed. ");
        }
    }

    LogMessage(L"Trying to install ProtonVPN TUN: ");
    Adapter = WintunCreateAdapter(AdapterPoolName, AdapterName, &AdapterGuid, nullptr);
    if (Adapter == nullptr)
    {
        LastError = GetLastError();
        LogMessage(L"Failed to create ProtonVPN TUN adapter: ", LastError);
    }
    else
    {
        WintunFreeAdapter(Adapter);
    }

    FreeLibrary(Wintun);

    return LastError;
}

int UninstallTunAdapter(LPCWSTR dllPath, BOOL* rebootRequired)
{
    DWORD LastError = ERROR_SUCCESS;
    HMODULE Wintun = InitializeWintun(dllPath);
    if (!Wintun)
    {
        LastError = GetLastError();
        LogMessage(L"Failed to initialize Wintun: ", LastError);
        return LastError;
    }

    WintunSetLogger(InstallLogger);

    WINTUN_ADAPTER_HANDLE Adapter = WintunOpenAdapter(AdapterPoolName, AdapterName);
    if (Adapter != nullptr)
    {
        WintunFreeAdapter(Adapter);

        if (WintunDeletePoolDriver(AdapterPoolName, rebootRequired) == 0)
        {
            LastError = GetLastError();
            LogMessage(L"Failed to uninstall ProtonVPN TUN adapter: ", LastError);
        }
        else
        {
            LogMessage(L"ProtonVPN TUN adapter uninstalled.");
        }
    }
    else
    {
        LogMessage(L"ProtonVPN TUN adapter is not installed. Skipping.");
    }

    FreeLibrary(Wintun);

    return LastError;
}