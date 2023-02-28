#pragma once
#include <string>
#include "ProcessExecutionResult.h"

using namespace std;

class TapInstaller
{
public:
    TapInstaller(const wchar_t* tap_files_path);
    DWORD Install();
    DWORD Uninstall();

private:
    DWORD ProcessDeviceErrorCode(ProcessExecutionResult result);
    bool IsVersionOutdated(string version);
    string GetInstalledAdapterVersion();

    ProcessExecutionResult RunCommand(wstring command);
    ProcessExecutionResult RunEnable();
    ProcessExecutionResult GetStatus();
    ProcessExecutionResult RunUpdate();
    ProcessExecutionResult RunInstall();
    ProcessExecutionResult RunRemove();
    ProcessExecutionResult GetDriverInfo();

    wstring GetInstallerPath();
    wstring GetDriverPath();
    bool IsRestartRequired(string output);
    bool IsDriverUpdateRequired(string output);
    bool IsDriverReinstallationRequired(string output);
    bool IsHealthyAndRunning(string output);
    bool IsDisabled(string output);

    wstring tap_files_path;
    const string TapVersion = "9.24.6.601";
    const wstring HardwareId = L"tapprotonvpn";
};
