#include <windows.h>
#include "ProcessExecutionResult.h"

ProcessExecutionResult::ProcessExecutionResult(std::string output, DWORD exitCode)
{
    this->output = output;
    this->exitCode = exitCode;
}

bool ProcessExecutionResult::is_success() const
{
    return exitCode == 0;
}

ProcessExecutionResult ProcessExecutionResult::Failure(DWORD exitCode)
{
    return {"", exitCode};
}