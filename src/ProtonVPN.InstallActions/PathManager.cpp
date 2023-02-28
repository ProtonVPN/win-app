#include "PathManager.h"

std::wstring AddEndingSlashIfNotExists(std::wstring directory)
{
    if (directory.empty())
    {
        directory = L"\\";
    }
    else if (directory[directory.size() - 1] != '\\')
    {
        directory += L"\\";
    }

    return directory;
}
