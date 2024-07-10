#pragma once
#include <Windows.h>

class ProcessResource
{
public:
    ProcessResource(HANDLE hProcess = nullptr, PPROC_THREAD_ATTRIBUTE_LIST pAttrList = nullptr);
    ~ProcessResource();

    void set_process_handle(HANDLE handle);
    void set_thread_handle(HANDLE handle);
    void set_attr_list(PPROC_THREAD_ATTRIBUTE_LIST attrList);

private:
    HANDLE hProcess;
    HANDLE hThread = nullptr;
    PPROC_THREAD_ATTRIBUTE_LIST pAttrList;
};