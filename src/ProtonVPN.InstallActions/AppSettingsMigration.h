#pragma once
#include <filesystem>
#include <string>
#include <vector>

#include "sha1.hpp"

class AppSettingsMigration
{
public:
    AppSettingsMigration();
    void SaveOldUserConfigFolder();
    void RestoreOldUserConfigFolder(std::string application_path);
    ~AppSettingsMigration();

private:
    char s_base32_char[32]
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
        'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
        'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
        'y', 'z', '0', '1', '2', '3', '4', '5'
    };

    SHA1* sha1;

    std::string ToBase32StringSuitableForDirName(std::vector<int> buff);
    std::vector<int> hex2bin(std::string hex);
    int char2int(char input);
    std::string GetSha1(std::string str);
    std::string GetPathWithSize(std::string path_url);
    std::string GetHashForUserSettingsFolder(std::string application_path);
    std::filesystem::path GetApplicationSettingsFolderPath(std::string application_path);
    std::filesystem::path GetTmpFolderPathForStorage();
    std::string GetApplicationSettingsFolderName(std::string application_path);
    std::filesystem::path GetOldUserConfigPath();
    void FixUserConfigFile(std::filesystem::path user_config_folder_path);
};
