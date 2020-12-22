#include "pch.h"
#include <winsock2.h>
#include <Windows.h>
#include "Utils.h"
#include "Wintun.h"

static WINTUN_CREATE_ADAPTER_FUNC WintunCreateAdapter;
static WINTUN_DELETE_ADAPTER_FUNC WintunDeleteAdapter;
static WINTUN_FREE_ADAPTER_FUNC WintunFreeAdapter;
static WINTUN_OPEN_ADAPTER_FUNC WintunOpenAdapter;

const WCHAR *AdapterName = L"ProtonVPN";
const WCHAR *AdapterDescription = L"ProtonVPN TUN";
const GUID AdapterGuid = { 0xafdeecba, 0xdfba, 0xcaff, {0x50, 0x44, 0x01, 0x34, 0x12, 0xbc, 0xea, 0xcd} };

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
        X(WintunDeleteAdapter, WINTUN_DELETE_ADAPTER_FUNC) ||
        X(WintunFreeAdapter, WINTUN_FREE_ADAPTER_FUNC) ||
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

    WINTUN_ADAPTER_HANDLE Adapter = WintunOpenAdapter(AdapterName, AdapterDescription);
    if (Adapter)
    {
        LogMessage(L"ProtonVPN TUN adapter already exists. Skipping.");
    }
    else
    {
        Adapter = WintunCreateAdapter(AdapterName, AdapterDescription, &AdapterGuid, nullptr);
        if (!Adapter)
        {
            LastError = GetLastError();
            LogMessage(L"Failed to create ProtonVPN TUN adapter: ", LastError);
        }
    }

    FreeLibrary(Wintun);

    return LastError;
}

int UninstallTunAdapter(LPCWSTR dllPath)
{
    DWORD LastError = ERROR_SUCCESS;
    HMODULE Wintun = InitializeWintun(dllPath);
    if (!Wintun)
    {
        LastError = GetLastError();
        LogMessage(L"Failed to initialize Wintun: ", LastError);
        return LastError;
    }

    WINTUN_ADAPTER_HANDLE Adapter = WintunOpenAdapter(AdapterName, AdapterDescription);
    if (Adapter)
    {
        if (WintunDeleteAdapter(Adapter, false, nullptr))
        {
            WintunFreeAdapter(Adapter);
            LogMessage(L"ProtonVPN TUN adapter uninstalled.");
        }
        else
        {
            LastError = GetLastError();
            LogMessage(L"Failed to uninstall ProtonVPN TUN adapter: ", LastError);
        }
    }
    else
    {
        LogMessage(L"ProtonVPN TUN adapter is not installed. Skipping.");
    }

    FreeLibrary(Wintun);

    return LastError;
}