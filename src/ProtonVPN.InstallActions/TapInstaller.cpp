#include <windows.h>
#include "TapInstaller.h"
#include <regex>
#include "Os.h"
#include "TapInstallationOutputParser.h"
#include "Utils.h"
#include <format>
#include <string>

using namespace std;

TapInstaller::TapInstaller(const wchar_t* p)
{
    this->tap_files_path = wstring(p) + L"\\";
}

DWORD TapInstaller::Install()
{
    const string version = GetInstalledAdapterVersion();
    bool is_to_install = false;

    if (version.empty())
    {
        LogMessage(L"TAP adapter is not installed.");
        is_to_install = true;
    }
    else
    {
        if (IsVersionOutdated(version))
        {
            const string str = std::format("Currently installed TAP adapter version {0} is outdated.", version);
            LogMessage(StrToConstWChar(str));
            is_to_install = true;
            Uninstall();
        }
    }

    ProcessExecutionResult driverStatus = ProcessExecutionResult({}, 0);
    if (is_to_install)
    {
        const ProcessExecutionResult result = RunInstall();
        driverStatus = GetStatus();
        if (result.exitCode != 0)
        {
            return ProcessDeviceErrorCode(driverStatus);
        }
    }
    else
    {
        driverStatus = GetStatus();
    }

    if (IsDisabled(driverStatus.output))
    {
        LogMessage(L"TAP driver is disabled.");
        RunEnable();
    }

    return 0;
}

DWORD TapInstaller::Uninstall()
{
    const string version = GetInstalledAdapterVersion();
    if (version.empty())
    {
        return 0;
    }

    return RunRemove().exitCode;
}

DWORD TapInstaller::ProcessDeviceErrorCode(ProcessExecutionResult result)
{
    if (IsHealthyAndRunning(result.output))
    {
        return 0;
    }

    if (IsDriverUpdateRequired(result.output))
    {
        LogMessage(L"TAP driver update required.");
        RunUpdate();
    }

    if (IsDriverReinstallationRequired(result.output))
    {
        LogMessage(L"TAP driver reinstall required.");
        Uninstall();
        return Install();
    }

    return result.exitCode;
}

bool TapInstaller::IsVersionOutdated(string version)
{
    return VersionCompare(TapVersion, version) == 1;
}

string TapInstaller::GetInstalledAdapterVersion()
{
    const ProcessExecutionResult result = GetDriverInfo();
    if (result.exitCode == 0)
    {
        const regex re(R"(Driver version is ([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+))");
        cmatch m;
        if (regex_search(result.output.c_str(), m, re))
        {
            return m[1].str();
        }
    }

    return {};
}

ProcessExecutionResult TapInstaller::RunCommand(wstring command)
{
    LogMessage(std::format(L"Executing command: {0} {1}", GetInstallerPath().c_str(), command));
    ProcessExecutionResult result = RunProcess(GetInstallerPath().c_str(), command);
    LogMessage(std::format(L"Command finished with exit code {0}, output: \n{1}", result.exitCode,
                           StrToConstWChar(result.output)));
    return result;
}

ProcessExecutionResult TapInstaller::RunEnable()
{
    return RunCommand(L" enable " + HardwareId);
}

ProcessExecutionResult TapInstaller::GetStatus()
{
    return RunCommand(L" status " + HardwareId);
}

ProcessExecutionResult TapInstaller::RunUpdate()
{
    return RunCommand(L" update \"" + GetDriverPath() + L"\" " + HardwareId);
}

ProcessExecutionResult TapInstaller::RunInstall()
{
    return RunCommand(L" install \"" + GetDriverPath() + L"\" " + HardwareId);
}

ProcessExecutionResult TapInstaller::RunRemove()
{
    return RunCommand(L" remove " + HardwareId);
}

ProcessExecutionResult TapInstaller::GetDriverInfo()
{
    return RunCommand(L" drivernodes " + HardwareId);
}

wstring TapInstaller::GetInstallerPath()
{
    return tap_files_path + L"tapinstall.exe";
}

wstring TapInstaller::GetDriverPath()
{
    return tap_files_path + L"OemVista.inf";
}

bool TapInstaller::IsRestartRequired(string output)
{
    const optional<int> code = TapInstallationOutputParser::ParseDeviceCode(output);
    if (code.has_value())
    {
        const int error_codes[] = {3, 14, 21, 28, 33, 38, 42, 44, 47, 54};
        return std::find(std::begin(error_codes), std::end(error_codes), code.value()) != std::end(error_codes);
    }

    return false;
}

bool TapInstaller::IsDriverUpdateRequired(string output)
{
    const optional<int> code = TapInstallationOutputParser::ParseDeviceCode(output);
    if (code.has_value())
    {
        const int error_codes[] = {1, 10, 18, 24, 31, 41, 48, 52};
        return std::find(std::begin(error_codes), std::end(error_codes), code.value()) != std::end(error_codes);
    }

    return false;
}

bool TapInstaller::IsDriverReinstallationRequired(string output)
{
    const optional<int> code = TapInstallationOutputParser::ParseDeviceCode(output);
    if (code.has_value())
    {
        const int error_codes[] = {3, 18, 19, 28, 32, 37, 39, 40};
        return std::find(std::begin(error_codes), std::end(error_codes), code.value()) != std::end(error_codes);
    }

    return false;
}

bool TapInstaller::IsHealthyAndRunning(string output)
{
    return TapInstallationOutputParser::ParseInstallerStatus(output) == DeviceExists;
}

bool TapInstaller::IsDisabled(string output)
{
    return TapInstallationOutputParser::ParseInstallerStatus(output) == DeviceIsDisabled;
}
