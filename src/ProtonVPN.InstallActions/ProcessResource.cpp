#include "ProcessResource.h"

ProcessResource::ProcessResource(HANDLE hProcess, PPROC_THREAD_ATTRIBUTE_LIST pAttrList)
    : hProcess(hProcess), pAttrList(pAttrList)
{
}

ProcessResource::~ProcessResource()
{
    if (hProcess)
    {
        CloseHandle(hProcess);
    }
    if (hThread)
    {
        CloseHandle(hThread);
    }
    if (pAttrList)
    {
        delete[] reinterpret_cast<char*>(pAttrList);
    }
}

void ProcessResource::set_process_handle(HANDLE handle)
{
    hProcess = handle;
}

void ProcessResource::set_thread_handle(HANDLE handle)
{
    hThread = handle;
}

void ProcessResource::set_attr_list(PPROC_THREAD_ATTRIBUTE_LIST attrList)
{
    pAttrList = attrList;
}