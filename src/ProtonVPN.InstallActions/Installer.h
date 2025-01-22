#pragma once

namespace Installer
{
    long Uninstall(LPCWSTR upgrade_code);
    bool IsProductInstalled(LPCWSTR upgrade_code);
}