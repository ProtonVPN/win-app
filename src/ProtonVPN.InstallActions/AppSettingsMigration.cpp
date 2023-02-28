#include <windows.h>
#include "sha1.hpp"
#include "WinApiErrorException.h"
#include "pugixml/pugixml.hpp"
#include "AppSettingsMigration.h"

using namespace std;
namespace fs = std::filesystem;

AppSettingsMigration::AppSettingsMigration()
{
    this->sha1 = new SHA1;
}

void AppSettingsMigration::SaveOldUserConfigFolder()
{
    const fs::path old_user_settings_path = GetOldUserConfigPath();
    if (!old_user_settings_path.empty())
    {
        try
        {
            const fs::path version_folder_path = GetTmpFolderPathForStorage() / old_user_settings_path.filename();
            fs::create_directories(version_folder_path);
            constexpr auto copy_options = fs::copy_options::update_existing | fs::copy_options::recursive;
            fs::copy(old_user_settings_path, version_folder_path, copy_options);
        }
        catch (fs::filesystem_error& e)
        {
            string s = string(e.what());
            throw WinApiErrorException(wstring(s.begin(), s.end()), 2);
        }
    }
}

void AppSettingsMigration::RestoreOldUserConfigFolder(std::string application_path)
{
    fs::path tmp_folder_path_for_storage = GetTmpFolderPathForStorage();
    if (!fs::exists(tmp_folder_path_for_storage))
    {
        return;
    }

    for (auto& p : fs::directory_iterator(tmp_folder_path_for_storage))
    {
        if (p.is_directory())
        {
            try
            {
                const fs::path settings_folder_path = GetApplicationSettingsFolderPath(application_path);
                const fs::path version_folder_path = settings_folder_path / p.path().filename();
                fs::create_directories(settings_folder_path);
                fs::copy(p, version_folder_path, fs::copy_options::update_existing);
                fs::remove_all(tmp_folder_path_for_storage);
                FixUserConfigFile(version_folder_path);
            }
            catch (fs::filesystem_error& e)
            {
                string s = string(e.what());
                throw WinApiErrorException(wstring(s.begin(), s.end()), 2);
            }

            break;
        }
    }
}

void AppSettingsMigration::FixUserConfigFile(fs::path user_config_folder_path)
{
    fs::path path = user_config_folder_path / fs::path("user.config");
    const wchar_t* user_config_path = path.c_str();

    pugi::xml_document doc;
    pugi::xml_parse_result result = doc.load_file(user_config_path, pugi::parse_full);

    pugi::xml_node configuration = doc.root().child("configuration").prepend_child(pugi::node_element);
    configuration.set_name("configSections");

    pugi::xml_node section_group = configuration.append_child(pugi::node_element);
    section_group.set_name("sectionGroup");
    section_group.append_attribute("name") = "userSettings";
    section_group.append_attribute("type") =
        "System.Configuration.UserSettingsGroup, System.Configuration.ConfigurationManager, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

    pugi::xml_node section = section_group.append_child(pugi::node_element);
    section.set_name("section");
    section.append_attribute("name") = "ProtonVPN.Properties.Settings";
    section.append_attribute("type") =
        "System.Configuration.ClientSettingsSection, System.Configuration.ConfigurationManager, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
    section.append_attribute("allowExeDefinition") = "MachineToLocalUser";
    section.append_attribute("requirePermission") = "false";

    doc.save_file(user_config_path);
}

fs::path AppSettingsMigration::GetOldUserConfigPath()
{
    const string local_app_data_path = GetLocalAppDataPath();
    fs::file_time_type latest_version;
    fs::path latest_version_folder_path;
    fs::path app_data_folder_path = local_app_data_path / fs::path("ProtonVPN");
    if (!fs::exists(app_data_folder_path))
    {
        return {};
    }

    for (auto& p : fs::directory_iterator(app_data_folder_path))
    {
        if (p.is_directory())
        {
            auto folder_name = p.path().filename().generic_string();
            if (folder_name.rfind("ProtonVPN.exe_Url_", 0) == 0)
            {
                for (auto& version_folder : fs::directory_iterator(p))
                {
                    if (version_folder.last_write_time() > latest_version)
                    {
                        latest_version = version_folder.last_write_time();
                        latest_version_folder_path = version_folder.path();
                    }
                }
                break;
            }
        }
    }

    return latest_version_folder_path;
}

fs::path AppSettingsMigration::GetApplicationSettingsFolderPath(std::string application_path)
{
    return {GetLocalAppDataPath() + "\\ProtonVPN\\" + GetApplicationSettingsFolderName(application_path)};
}

fs::path AppSettingsMigration::GetTmpFolderPathForStorage()
{
    return GetTmpFolderPath() / fs::path("ProtonVPN");
}

string AppSettingsMigration::GetApplicationSettingsFolderName(std::string application_path)
{
    return "ProtonVPN_Url_" + GetHashForUserSettingsFolder(application_path);
}

string AppSettingsMigration::GetLocalAppDataPath()
{
    return GetEnvVariable("LOCALAPPDATA");
}

string AppSettingsMigration::GetTmpFolderPath()
{
    return GetEnvVariable("TMP");
}

string AppSettingsMigration::GetEnvVariable(string name)
{
    char* value;
    size_t len;
    const errno_t err = _dupenv_s(&value, &len, name.c_str());
    if (err != 0)
    {
        throw WinApiErrorException(L"Failed to get environment variable " + wstring(name.begin(), name.end()) + L".",
                                   err);
    }

    string result = string(value);
    free(value);

    return result;
}

string AppSettingsMigration::GetHashForUserSettingsFolder(std::string application_path)
{
    ranges::replace(application_path, '\\', '/');
    string path_url = string("FILE:///") + application_path + "/PROTONVPN.DLL";
    ranges::for_each(path_url, [](char& c) { c = ::toupper(c); });

    path_url = GetPathWithSize(path_url);
    string hash = GetSha1(path_url);

    return ToBase32StringSuitableForDirName(hex2bin(hash));
}

string AppSettingsMigration::GetPathWithSize(string path_url)
{
    vector<int> bytes;
    bytes.push_back(path_url.size());
    for (const char i : path_url)
    {
        bytes.push_back(i);
    }

    return {bytes.begin(), bytes.end()};
}

string AppSettingsMigration::GetSha1(string str)
{
    sha1->update(str);
    return sha1->final();
}

int AppSettingsMigration::char2int(char input)
{
    if (input >= '0' && input <= '9')
    {
        return input - '0';
    }

    if (input >= 'A' && input <= 'F')
    {
        return input - 'A' + 10;
    }

    if (input >= 'a' && input <= 'f')
    {
        return input - 'a' + 10;
    }

    throw invalid_argument("Invalid input string");
}

vector<int> AppSettingsMigration::hex2bin(string hex)
{
    const char* src = hex.c_str();
    vector<int> result;

    while (*src && src[1])
    {
        result.push_back(char2int(*src) * 16 + char2int(src[1]));
        src += 2;
    }

    return result;
}

string AppSettingsMigration::ToBase32StringSuitableForDirName(vector<int> buff)
{
    const auto length = buff.size();
    size_t i = 0;

    stringstream ss;
    do
    {
        int b0 = (i < length) ? buff[i++] : 0;
        int b1 = (i < length) ? buff[i++] : 0;
        int b2 = (i < length) ? buff[i++] : 0;
        int b3 = (i < length) ? buff[i++] : 0;
        int b4 = (i < length) ? buff[i++] : 0;

        ss << s_base32_char[b0 & 0x1F];
        ss << s_base32_char[b1 & 0x1F];
        ss << s_base32_char[b2 & 0x1F];
        ss << s_base32_char[b3 & 0x1F];
        ss << s_base32_char[b4 & 0x1F];

        ss << s_base32_char[(
            ((b0 & 0xE0) >> 5) |
            ((b3 & 0x60) >> 2))];

        ss << s_base32_char[(
            ((b1 & 0xE0) >> 5) |
            ((b4 & 0x60) >> 2))];

        b2 >>= 5;

        if ((b3 & 0x80) != 0)
        {
            b2 |= 0x08;
        }

        if ((b4 & 0x80) != 0)
        {
            b2 |= 0x10;
        }

        ss << s_base32_char[b2];
    }
    while (i < length);

    return ss.str();
}

AppSettingsMigration::~AppSettingsMigration()
{
    delete sha1;
}
