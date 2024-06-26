#pragma once
#include "ProcessExecutionResult.h"

namespace Os
{
    ProcessExecutionResult RunProcess(const wchar_t* application_path, std::wstring command_line_args);
    ProcessExecutionResult LaunchUnelevatedProcess(const wchar_t* processPath, const wchar_t* args, bool is_to_wait);
    bool IsProcessRunning(const wchar_t* process_name);
    bool IsProcessRunningByPath(const std::wstring& processPath);
    std::string GetEnvVariable(std::string name);
    long ChangeShortcutTarget(const wchar_t* shortcut_path, const wchar_t* target_path);
    void RemovePinnedIcons(PCWSTR shortcut_path);

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