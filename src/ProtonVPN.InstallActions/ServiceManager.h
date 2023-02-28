#pragma once
#include <string>
#include <wil/resource.h>

class ServiceManager
{
public:
    void Create(std::wstring name, std::wstring displayName, std::wstring file_path, DWORD serviceType);
    void Delete(std::wstring serviceName);
    void Stop(wil::unique_schandle& handle, std::wstring name);
    bool Exists(std::wstring name);

private:
    const DWORD timeout_ = 30000;
    wil::unique_schandle GetServiceManagerHandle();
    wil::unique_schandle GetServiceHandle(wil::unique_schandle& handle, std::wstring name, DWORD access);
    void ModifyPermissions(wil::unique_schandle& handle, std::wstring name);
    SERVICE_STATUS_PROCESS GetStatus(wil::unique_schandle& handle);
    DWORD GetWaitTime(SERVICE_STATUS_PROCESS status);
};
