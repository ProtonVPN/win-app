#pragma once

#include <Windows.h>
#include <vector>
#include <string>

class BestInterface
{
public:
    static IN_ADDR IpAddress(const std::vector<std::wstring>& excludedIfaceGuids);
};
