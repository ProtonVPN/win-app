#pragma once
#include "ProcessExecutionResult.h"

namespace Os
{
    ProcessExecutionResult RunProcess(const wchar_t* application_path, std::wstring command_line_args);
    bool IsProcessRunning(const wchar_t* process_name);
    std::string GetLocalAppDataPath();
    std::string GetTmpFolderPath();
    std::string GetEnvVariable(std::string name);
    long ChangeShortcutTarget(const wchar_t* shortcut_path, const wchar_t* target_path);

    enum DriverState
    {
        DeviceHasAProblem,
        DeviceIsDisabled,
        DeviceIsStopped,
        NoDeviceFound,
        DeviceExists,
        Unknown
    };
}