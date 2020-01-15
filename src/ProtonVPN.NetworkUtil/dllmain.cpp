#include "StdAfx.h"
#include <comdef.h>
#include "NetworkConfiguration.h"
#include "NetworkIPv6Settings.h"
#include "Route.h"
#include "BestInterface.h"

#define EXPORT __declspec(dllexport)

const DWORD LockTimeoutMs = 5000;

extern "C" EXPORT long NetworkUtilEnableIPv6OnAllAdapters(wchar_t* appName)
{
    try
    {
        auto networkConfig = Proton::NetworkUtil::NetworkConfiguration::instance();
        auto lock = networkConfig.acquireWriteLock(LockTimeoutMs, appName);
        networkConfig.initialize();

        networkConfig.ipv6Settings().enableIPv6OnAllAdapters(true);
        networkConfig.applyChanges();

        lock->ReleaseWriteLock();
    }
    catch (const _com_error& error)
    {
        return error.Error();
    }

    return 0;
}

extern "C" EXPORT long NetworkUtilDisableIPv6OnAllAdapters(wchar_t* appName)
{
    try
    {
        auto networkConfig = Proton::NetworkUtil::NetworkConfiguration::instance();
        auto lock = networkConfig.acquireWriteLock(LockTimeoutMs, appName);
        networkConfig.initialize();

        networkConfig.ipv6Settings().enableIPv6OnAllAdapters(false);
        networkConfig.applyChanges();

        lock->ReleaseWriteLock();
    }
    catch (const _com_error& error)
    {
        return error.Error();
    }

    return 0;
}

extern "C" EXPORT DWORD AddRoute(PCWSTR address)
{
    Route route;
    return route.Add(IpAddress(address), IpAddress(L"255.255.255.255"));
}

extern "C" EXPORT DWORD DeleteRoute(PCWSTR address)
{
    Route route;
    return route.Delete(IpAddress(address), IpAddress(L"255.255.255.255"));
}

extern "C" EXPORT DWORD GetBestInterfaceIp(IN_ADDR* address)
{
    BestInterface bestInterface;
    *address = bestInterface.IpAddress();

    return 0;
}
