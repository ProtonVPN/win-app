#include <windows.h>
#include "TapInstallationOutputParser.h"
#include <optional>
#include <regex>
#include "Os.h"
#include "Utils.h"

DriverState TapInstallationOutputParser::ParseInstallerStatus(string output)
{
    if (FindCaseInsensitive(output, "The device has the following problem"))
    {
        return DeviceHasAProblem;
    }

    if (FindCaseInsensitive(output, "Device is disabled"))
    {
        return DeviceIsDisabled;
    }

    if (FindCaseInsensitive(output, "Device is currently stopped"))
    {
        return DeviceIsStopped;
    }

    if (FindCaseInsensitive(output, "No matching devices found"))
    {
        return NoDeviceFound;
    }

    if (FindCaseInsensitive(output, " matching device(s) found"))
    {
        return DeviceExists;
    }

    return Os::Unknown;
}

optional<int> TapInstallationOutputParser::ParseDeviceCode(string output)
{
    const regex re(R"(: ?(\\d+))");
    cmatch m;
    if (regex_search(output.c_str(), m, re))
    {
        return stoi(m[1].str());
    }

    return {};
}
