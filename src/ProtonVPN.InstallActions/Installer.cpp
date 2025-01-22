#include <windows.h>
#include "Msi.h"
#include "Utils.h"

#define MSI_GUID_LEN 38

namespace Installer
{
    long Uninstall(LPCWSTR upgrade_code)
    {
        DWORD index = 0;
        TCHAR product_code[MSI_GUID_LEN + 1];
        int total_removed = 0;

        while (MsiEnumRelatedProducts(upgrade_code, 0, index++, product_code) == ERROR_SUCCESS)
        {
            const auto args = L"REBOOT=ReallySuppress UILevel=2"; // 2 - INSTALLUILEVEL_NONE
            UINT res = MsiConfigureProductEx(product_code, INSTALLLEVEL_DEFAULT, INSTALLSTATE_ABSENT, args);
            std::wstring product_code_string = product_code;

            if (res == ERROR_SUCCESS)
            {
                total_removed++;
                LogMessage(L"Successfully uninstalled product code: " + product_code_string);
            }
            else
            {
                LogMessage(L"Failed to uninstall product code " + product_code_string + L": ", res);

                return res;
            }
        }

        if (total_removed == 0)
        {
            LogMessage(L"No product found with product code " + static_cast<std::wstring>(upgrade_code));
        }

        return 0;
    }

    bool IsProductInstalled(LPCWSTR upgrade_code)
    {
        TCHAR product_code[MSI_GUID_LEN + 1];
        return MsiEnumRelatedProducts(upgrade_code, 0, 0, product_code) == ERROR_SUCCESS;
    }
}