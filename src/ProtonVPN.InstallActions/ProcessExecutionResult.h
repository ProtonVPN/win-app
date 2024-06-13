#pragma once
#include <string>
#include <minwindef.h>

class ProcessExecutionResult
{
public:
    ProcessExecutionResult(std::string output, DWORD exitCode);
    bool is_success() const;
    std::string output;
    DWORD exitCode;
    static ProcessExecutionResult Failure(DWORD exitCode);
};