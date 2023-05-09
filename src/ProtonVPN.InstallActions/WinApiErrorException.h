#pragma once
#include <exception>
#include <string>
#include <minwindef.h>

class WinApiErrorException : public std::exception
{
public:
    WinApiErrorException(std::wstring message, DWORD error_code);
    DWORD GetErrorCode();
    std::wstring GetError();

private:
    std::wstring message_;
    DWORD error_code_;
};
