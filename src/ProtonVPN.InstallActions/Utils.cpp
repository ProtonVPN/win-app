#include <windows.h>
#include <algorithm>
#include <functional>
#include "Logger.h"
#include "WinApiErrorException.h"

void LogMessage(std::wstring message, UINT result)
{
    logger((message + L" Error code: " + std::to_wstring(result)).c_str());
}

void LogMessage(std::wstring message)
{
    logger(message.c_str());
}

int VersionCompare(std::string v1, std::string v2)
{
    int vnum1 = 0, vnum2 = 0;
    for (UINT i = 0, j = 0; i < v1.length() || j < v2.length();)
    {
        while (i < v1.length() && v1[i] != '.')
        {
            vnum1 = vnum1 * 10 + (v1[i] - '0');
            i++;
        }

        while (j < v2.length() && v2[j] != '.')
        {
            vnum2 = vnum2 * 10 + (v2[j] - '0');
            j++;
        }

        if (vnum1 > vnum2)
        {
            return 1;
        }

        if (vnum2 > vnum1)
        {
            return -1;
        }

        vnum1 = vnum2 = 0;
        i++;
        j++;
    }

    return 0;
}

bool FindCaseInsensitive(std::string data, std::string toSearch)
{
    std::transform(data.begin(), data.end(), data.begin(), ::tolower);
    std::transform(toSearch.begin(), toSearch.end(), toSearch.begin(), ::tolower);
    return data.find(toSearch, 0) != std::string::npos;
}

std::wstring StrToConstWChar(std::string str)
{
    return std::wstring(str.begin(), str.end());
}

DWORD ExecuteAction(const std::function<void()>& func)
{
    try
    {
        func();
        return 0;
    }
    catch (WinApiErrorException& e)
    {
        LogMessage(e.GetError(), e.GetErrorCode());
        return e.GetErrorCode();
    }
}
