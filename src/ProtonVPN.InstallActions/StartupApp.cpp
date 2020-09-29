#include "pch.h"
#include "Utils.h"

void add_installer_to_startup()
{
    const auto installerPath = GetProperty(L"SETUPEXEDIR") + GetProperty(L"SetupExe");

    TCHAR tmp_path[MAX_PATH];
    GetTempPath(MAX_PATH, tmp_path);

    const std::wstring tmp_path_str(tmp_path);
    const auto base_filename = installerPath.substr(installerPath.find_last_of(L"\\\\") + 1);
    const auto tmp_new_path = tmp_path_str + base_filename;
    const auto new_path = std::wstring(tmp_new_path.begin(), tmp_new_path.end());

    LogMessage(L"Origin installer path: " + installerPath);
    LogMessage(L"New tmp path: " + new_path);

    CopyFile(installerPath.c_str(), new_path.c_str(), false);
    HKEY h_key = nullptr;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, TEXT("Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"), 0, KEY_WRITE | KEY_WOW64_64KEY, &h_key);
    if (h_key != nullptr)
    {
        RegSetValueEx(h_key, nullptr, 0, REG_SZ, reinterpret_cast<const BYTE*>(new_path.c_str()), (new_path.length() + 1) * sizeof(wchar_t));
        RegCloseKey(h_key);
    }
}
