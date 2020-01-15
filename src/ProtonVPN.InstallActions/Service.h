#pragma once
#include <string>

int ModifyServicePermissions(std::wstring serviceName);
int DeleteService(std::wstring serviceName);
int CreateDriverService(std::wstring serviceName, std::wstring displayName, std::wstring driverPath);
