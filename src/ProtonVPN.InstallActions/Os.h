#pragma once
#include "ProcessExecutionResult.h"

namespace Os
{
    ProcessExecutionResult RunProcess(const wchar_t* application_path, std::wstring command_line_args);
    bool IsProcessRunning(const wchar_t* process_name);

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
