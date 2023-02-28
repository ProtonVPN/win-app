#include <windows.h>
#include "WinApiErrorException.h"

WinApiErrorException::WinApiErrorException(std::wstring message, DWORD error_code)
{
    this->message_ = message;
    this->error_code_ = error_code;
}

std::wstring WinApiErrorException::GetError()
{
    return message_;
}

DWORD WinApiErrorException::GetErrorCode()
{
    return error_code_;
}
