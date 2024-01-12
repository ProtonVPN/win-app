#include <windows.h>
#include "WinApiErrorException.h"
#include "AppSettingsMigration.h"
#include "Os.h"

using namespace std;
namespace fs = std::filesystem;

void AppSettingsMigration::SaveOldUserConfigFile()
{
    const fs::path old_user_settings_path = GetOldUserConfigPath();
    if (!old_user_settings_path.empty())
    {
        try
        {
            const fs::path user_config_file = fs::path("user.config");
            const fs::path user_config_path = old_user_settings_path / user_config_file;
            const wchar_t* user_config_path_str = user_config_path.c_str();

            fs::path new_user_folder = Os::GetProgramDataPath() / fs::path("Proton") / fs::path("Proton VPN");
            if (!fs::exists(new_user_folder))
            {
                fs::create_directories(new_user_folder);
            }

            fs::copy_file(user_config_path_str, new_user_folder / user_config_file, fs::copy_options::overwrite_existing);
        }
        catch (fs::filesystem_error& e)
        {
            string s = string(e.what());
            throw WinApiErrorException(wstring(s.begin(), s.end()), 2);
        }
    }
}

fs::path AppSettingsMigration::GetOldUserConfigPath()
{
    const string local_app_data_path = Os::GetLocalAppDataPath();
    const fs::path app_data_folder_path = local_app_data_path / fs::path("ProtonVPN");

    if (!fs::exists(app_data_folder_path))
    {
        return {};
    }

    // v3
    fs::path latest_version_folder_path = GetOldUserConfigPathByFolderPrefix(app_data_folder_path, "ProtonVPN_Url_");
    if (latest_version_folder_path.empty())
    {
        // v2
        latest_version_folder_path = GetOldUserConfigPathByFolderPrefix(app_data_folder_path, "ProtonVPN.exe_Url_");
    }

    return latest_version_folder_path;
}

fs::path AppSettingsMigration::GetOldUserConfigPathByFolderPrefix(const fs::path& app_data_folder_path, const string& prefix)
{
    fs::file_time_type latest_version_time;
    fs::path latest_version_folder_path;

    for (auto& p : fs::directory_iterator(app_data_folder_path))
    {
        if (p.is_directory())
        {
            auto folder_name = p.path().filename().generic_string();
            if (folder_name.rfind(prefix, 0) == 0)
            {
                for (auto& version_folder : fs::directory_iterator(p))
                {
                    if (version_folder.last_write_time() > latest_version_time)
                    {
                        latest_version_time = version_folder.last_write_time();
                        latest_version_folder_path = version_folder.path();
                    }
                }
            }
        }
    }

    return latest_version_folder_path;
}