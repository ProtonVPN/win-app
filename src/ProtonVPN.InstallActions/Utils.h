#pragma once
#include <functional>
#include <string>

using namespace std;

void LogMessage(std::wstring message, UINT result);
void LogMessage(std::wstring message);
int VersionCompare(std::string v1, std::string v2);
bool FindCaseInsensitive(string data, string toSearch);
std::wstring StrToConstWChar(string str);
DWORD ExecuteAction(const function<void()>& func);
