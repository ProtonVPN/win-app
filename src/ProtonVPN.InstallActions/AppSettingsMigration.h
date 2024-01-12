#pragma once
#include <filesystem>
#include <string>

class AppSettingsMigration
{
public:
    void SaveOldUserConfigFile();

private:
    std::filesystem::path GetApplicationSettingsFolderPath(std::string application_path);
    std::string GetApplicationSettingsFolderName(std::string application_path);
    std::filesystem::path GetOldUserConfigPath();
    std::filesystem::path GetOldUserConfigPathByFolderPrefix(const std::filesystem::path& app_data_folder_path, const std::string& prefix);
};