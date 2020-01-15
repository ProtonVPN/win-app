#pragma once
#include <string>
#include <winnt.h>

class tmp_string
{
private:
    std::basic_string<TCHAR>& value;
    TCHAR* buff;

public:
    tmp_string(std::basic_string<TCHAR>& result, int size) : value(result)
    {
        buff = new TCHAR[size]{};
    }

    tmp_string(tmp_string& c) = delete;

    tmp_string& operator=(tmp_string& c) = delete;

    ~tmp_string()
    {
        value = std::basic_string<TCHAR>(buff);
        delete buff;
    }

    operator LPTSTR() const
    {
        return buff;
    }
};
