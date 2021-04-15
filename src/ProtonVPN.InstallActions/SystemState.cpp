#include "pch.h"
#include "winreg.h"

HKEY get_key(const LPCWSTR key)
{
	HKEY h_key = nullptr;
	RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, KEY_READ | KEY_WOW64_64KEY, &h_key);

	return h_key;
}

bool run_once_check()
{
	const auto key = get_key(TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"));
	if (key != nullptr)
	{
		WCHAR sz_buffer[512];
		DWORD dw_buffer_size = sizeof sz_buffer;
		const auto result = RegQueryValueEx(key, nullptr, nullptr, nullptr, reinterpret_cast<LPBYTE>(sz_buffer),
		                                    &dw_buffer_size);
		if (result == ERROR_SUCCESS && wcscmp(sz_buffer, L"DVDRebootSignal") == 0)
		{
			RegCloseKey(key);
			return true;
		}

		RegCloseKey(key);
	}

	return false;
}

bool check_keys_exist()
{
	LPCWSTR keys_exist[] = {
		TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\RebootPending"),
		TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\RebootInProgress"),
		TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\RebootRequired"),
		TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\PackagesPending"),
		TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\PostRebootReporting"),
	};

	for (const auto key : keys_exist)
	{
		const auto key_p = get_key(key);
		if (key_p != nullptr)
		{
			RegCloseKey(key_p);
			return true;
		}
	}

	return false;
}

bool pending_update()
{
	const auto key = get_key(TEXT("SOFTWARE\\Microsoft\\Updates"));
	if (key != nullptr)
	{
		WCHAR sz_buffer[512];
		DWORD dw_buffer_size = sizeof sz_buffer;
		const auto result = RegQueryValueEx(
			key,
			TEXT("UpdateExeVolatile"),
			nullptr,
			nullptr,
			reinterpret_cast<LPBYTE>(sz_buffer),
			&dw_buffer_size);

		if (result == ERROR_SUCCESS && wcscmp(sz_buffer, L"0") != 0)
		{
			RegCloseKey(key);
			return true;
		}

		RegCloseKey(key);
	}

	const auto updatePendingKey = get_key(TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Services\\Pending"));
	if (updatePendingKey != nullptr)
	{
		DWORD total_subkeys = 0;

		RegQueryInfoKey(updatePendingKey,
			nullptr,
			nullptr,
			nullptr,
			&total_subkeys,
			nullptr,
			nullptr,
			nullptr,
			nullptr,
			nullptr,
			nullptr,
			nullptr);

		if (total_subkeys > 0)
		{
			RegCloseKey(updatePendingKey);
			return true;
		}

		RegCloseKey(updatePendingKey);
	}

	return false;
}

bool pending_reboot()
{
	return check_keys_exist() ||
		run_once_check() ||
		pending_update();
}
