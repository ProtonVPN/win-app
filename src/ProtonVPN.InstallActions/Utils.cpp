#include "pch.h"
#include <string>
#include <msiquery.h>
#include "StringHelper.h"

MSIHANDLE msiHandle;

void SetMsiHandle(MSIHANDLE hInstall)
{
    msiHandle = hInstall;
}

void LogMessage(std::wstring message, int result)
{
    const MSIHANDLE hRecord = MsiCreateRecord(1);
    MsiRecordSetString(hRecord, 0, (message + std::to_wstring(result)).c_str());
    MsiProcessMessage(msiHandle, INSTALLMESSAGE_INFO, hRecord);
    MsiCloseHandle(hRecord);
}

void LogMessage(std::wstring message)
{
    const MSIHANDLE hRecord = MsiCreateRecord(1);
    MsiRecordSetString(hRecord, 0, message.c_str());
    MsiProcessMessage(msiHandle, INSTALLMESSAGE_INFO, hRecord);
    MsiCloseHandle(hRecord);
}

std::wstring GetProperty(std::wstring name)
{
    DWORD valueBufSize = 0;
    std::wstring valueBuf;

    auto result = MsiGetProperty(msiHandle, name.c_str(), tmp_string(valueBuf, valueBufSize), &valueBufSize);

    if (result == ERROR_MORE_DATA)
    {
        // valueBufSize now contains the size of the property's string, without null termination
        // Add 1 for null termination
        ++valueBufSize;
        result = MsiGetProperty(msiHandle, name.c_str(), tmp_string(valueBuf, valueBufSize), &valueBufSize);
    }

    if (result != ERROR_SUCCESS)
    {
        return std::wstring();
    }

    valueBuf[valueBufSize] = '\0';

    return valueBuf;
}

void SetProperty(const std::wstring name, const std::wstring value)
{
    MsiSetProperty(msiHandle, name.c_str(), value.c_str());
}
