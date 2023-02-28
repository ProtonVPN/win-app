#pragma once
#include <string>
#include <minwindef.h>

class ProcessExecutionResult
{
public:
    ProcessExecutionResult(std::string output, DWORD exitCode);
    std::string output;
    DWORD exitCode;
    static ProcessExecutionResult Failure(DWORD exitCode);
};
