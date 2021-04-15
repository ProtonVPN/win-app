#include "pch.h"
#include "Msi.h"
#include "Utils.h"
#define MSI_GUID_LEN 38

long Uninstall(LPCWSTR upgradeCode)
{
    DWORD index = 0;
    TCHAR productCode[MSI_GUID_LEN + 1];
    int totalRemoved = 0;

    while (MsiEnumRelatedProducts(upgradeCode, 0, index++, productCode) == ERROR_SUCCESS)
    {
        const auto args = L"REBOOT=ReallySuppress UILevel=2"; // 2 - INSTALLUILEVEL_NONE
        UINT res = MsiConfigureProductEx(productCode, INSTALLLEVEL_DEFAULT, INSTALLSTATE_ABSENT, args);
        std::wstring productCodeString = static_cast<std::wstring>(productCode);

        if (res == ERROR_SUCCESS)
        {
            totalRemoved++;
            LogMessage(L"Successfully uninstalled product code: " + productCodeString);
        }
        else
        {
            LogMessage(L"Failed to uninstall product code " + productCodeString + L": ", res);

            return res;
        }
    }

    if (totalRemoved == 0)
    {
        LogMessage(L"No product found with product code " + static_cast<std::wstring>(upgradeCode));
    }

    return 0;
}