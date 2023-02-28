#include <windows.h>
#include "ProcessExecutionResult.h"

ProcessExecutionResult::ProcessExecutionResult(std::string output, DWORD exitCode)
{
    this->output = output;
    this->exitCode = exitCode;
}

ProcessExecutionResult ProcessExecutionResult::Failure(DWORD exitCode)
{
    return {"", exitCode};
}
