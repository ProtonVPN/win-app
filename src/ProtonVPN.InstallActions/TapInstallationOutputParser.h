#pragma once
#include <string>
#include <optional>
#include "Os.h"

using namespace std;
using namespace Os;

class TapInstallationOutputParser
{
public:
    static DriverState ParseInstallerStatus(string output);
    static optional<int> ParseDeviceCode(string output);
};
