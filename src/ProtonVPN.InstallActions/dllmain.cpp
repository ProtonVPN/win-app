#include <filesystem>
#include <functional>
#include <string>
#include <strsafe.h>
#include "ServiceManager.h"
#include "Installer.h"
#include "TapInstaller.h"
#include "ip_filter.h"
#include "Logger.h"
#include "Os.h"
#include "Utils.h"
#include "AppSettingsMigration.h"
#include <string>

#define EXPORT __declspec(dllexport)

using namespace std;
namespace fs = std::filesystem;

GUID providerGUID = {0x20865f68, 0x0b04, 0x44da, {0xbb, 0x83, 0x22, 0x38, 0x62, 0x25, 0x40, 0xfa}};
GUID sublayerGUID = {0xaa867e71, 0x5765, 0x4be3, {0x93, 0x99, 0x58, 0x15, 0x85, 0xc2, 0x26, 0xce}};

extern "C" EXPORT void InitLogger(LoggerFunc loggerFunc)
{
    logger = loggerFunc;
}

extern "C" EXPORT long RemoveWfpObjects()
{
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

extern "C" EXPORT DWORD UninstallProduct(const wchar_t* upgrade_code)
{
    return ExecuteAction([upgrade_code]
    {
        return Installer::Uninstall(upgrade_code);
    });
}

extern "C" EXPORT DWORD IsProductInstalled(const wchar_t* upgrade_code)
{
    return Installer::IsProductInstalled(upgrade_code);
}

extern "C" EXPORT DWORD InstallService(const wchar_t* name, const wchar_t* display_name, const wchar_t* path)
{
    return ExecuteAction([name, display_name, path]
    {
        ServiceManager service_manager;
        service_manager.Create(name, display_name, path, SERVICE_WIN32_OWN_PROCESS);
    });
}

extern "C" EXPORT DWORD UninstallService(const wchar_t* name)
{
    return ExecuteAction([name]
    {
        ServiceManager service_manager;
        return service_manager.Delete(name);
    });
}

extern "C" EXPORT DWORD InstallCalloutDriver(const wchar_t* name, const wchar_t* display_name, const wchar_t* path)
{
    return ExecuteAction([name, display_name, path]
    {
        ServiceManager service_manager;
        return service_manager.Create(name, display_name, path, SERVICE_KERNEL_DRIVER);
    });
}

extern "C" EXPORT DWORD InstallTapAdapter(const wchar_t* tap_files_path)
{
    TapInstaller tap_installer(tap_files_path);
    return tap_installer.Install();
}

extern "C" EXPORT DWORD UninstallTapAdapter(const wchar_t* tap_files_path)
{
    TapInstaller tap_installer(tap_files_path);
    return tap_installer.Uninstall();
}

extern "C" EXPORT bool IsProcessRunning(const wchar_t* process_name)
{
    return Os::IsProcessRunning(process_name);
}

extern "C" EXPORT bool IsProcessRunningByPath(const wchar_t* process_path)
{
    return Os::IsProcessRunningByPath(process_path);
}

extern "C" EXPORT DWORD SaveOldUserConfigFolder()
{
    return ExecuteAction([]
    {
        AppSettingsMigration app_settings;
        app_settings.SaveOldUserConfigFolder();

        return 0;
    });
}

extern "C" EXPORT DWORD RestoreOldUserConfigFolder(const wchar_t* application_path)
{
    return ExecuteAction([application_path]
    {
        std::wstring w_path(application_path);
        AppSettingsMigration app_settings;
        app_settings.RestoreOldUserConfigFolder(string(w_path.begin(), w_path.end()));

        return 0;
    });
}

extern "C" EXPORT DWORD UpdateTaskbarIconTarget(const wchar_t* launcher_path)
{
    fs::path taskbar_path = fs::path(Os::GetEnvVariable("APPDATA")) / fs::path(L"Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");
    fs::path v2_shorcut_path = fs::path(taskbar_path) / fs::path(L"Proton VPN.lnk");
    fs::path v3_shortcut_path = fs::path(taskbar_path) / fs::path(L"ProtonVPN.lnk");
    fs::path shortcut_path;

    if (fs::exists(v2_shorcut_path))
    {
        shortcut_path = v2_shorcut_path;
    }
    else if (fs::exists(v3_shortcut_path))
    {
        shortcut_path = v3_shortcut_path;
    }

    return shortcut_path.empty() ? 0 : Os::ChangeShortcutTarget(shortcut_path.c_str(), launcher_path);
}

extern "C" EXPORT DWORD SetUpdatesFolderPermission(const wchar_t* folder_path)
{
    const size_t folder_path_length = wcslen(folder_path);
    wchar_t* path = new wchar_t[folder_path_length + 1];

    if (wcscpy_s(path, folder_path_length + 1, folder_path) == 0)
    {
        wstring securityDescriptor =
            L"D:(A;;0x1301bf;;;SY)"
            "(A;OICIIO;GA;;;SY)"
            "(A;;0x1301bf;;;BA)"
            "(A;OICIIO;GA;;;BA)"
            "(A;;0x1200a9;;;BU)"
            "(A;OICIIO;GXGR;;;BU)"
            "(A;OICIIO;GA;;;CO)"
            "(A;;0x1200a9;;;AC)"
            "(A;OICIIO;GXGR;;;AC)";
        Os::SetFolderPermissions(path, securityDescriptor.c_str());
    }
    else
    {
        LogMessage(L"Failed to copy string.");
    }

    return 0;
}

extern "C" EXPORT DWORD RemovePinnedIcons(const wchar_t* shortcut_path)
{
    Os::RemovePinnedIcons(shortcut_path);
    return 0;
}