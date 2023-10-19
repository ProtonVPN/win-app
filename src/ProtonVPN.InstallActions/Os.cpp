#include <string>
#include <windows.h>
#include <cstdint>
#include <string>
#include <thread>
#include <tlhelp32.h>
#include <shobjidl_core.h>
#include "shlguid.h"
#include "psapi.h"
#include <filesystem>

#include "ProcessExecutionResult.h"
#include "Os.h"
#include <sddl.h>
#include <shobjidl.h>

#include "Utils.h"

#include "WinApiErrorException.h"

using namespace std;
using namespace Os;

ProcessExecutionResult Os::RunProcess(const wchar_t* application_path, wstring command_line_args)
{
    SECURITY_ATTRIBUTES security_attributes;
    HANDLE stdout_rd = INVALID_HANDLE_VALUE;
    HANDLE stdout_wr = INVALID_HANDLE_VALUE;
    HANDLE stderr_rd = INVALID_HANDLE_VALUE;
    HANDLE stderr_wr = INVALID_HANDLE_VALUE;
    PROCESS_INFORMATION process_info;
    STARTUPINFO startup_info;
    thread stdout_thread;
    thread stderr_thread;

    security_attributes.nLength = sizeof(SECURITY_ATTRIBUTES);
    security_attributes.bInheritHandle = TRUE;
    security_attributes.lpSecurityDescriptor = nullptr;

    if (!CreatePipe(&stdout_rd, &stdout_wr, &security_attributes, 0) ||
        !SetHandleInformation(stdout_rd, HANDLE_FLAG_INHERIT, 0))
    {
        return ProcessExecutionResult::Failure(-1);
    }

    if (!CreatePipe(&stderr_rd, &stderr_wr, &security_attributes, 0) ||
        !SetHandleInformation(stderr_rd, HANDLE_FLAG_INHERIT, 0))
    {
        if (stdout_rd != INVALID_HANDLE_VALUE) CloseHandle(stdout_rd);
        if (stdout_wr != INVALID_HANDLE_VALUE) CloseHandle(stdout_wr);
        return ProcessExecutionResult::Failure(-2);
    }

    ZeroMemory(&process_info, sizeof(PROCESS_INFORMATION));
    ZeroMemory(&startup_info, sizeof(STARTUPINFO));

    startup_info.cb = sizeof(STARTUPINFO);
    startup_info.hStdInput = nullptr;
    startup_info.hStdOutput = stdout_wr;
    startup_info.hStdError = stderr_wr;

    if (stdout_rd || stderr_rd)
    {
        startup_info.dwFlags |= STARTF_USESTDHANDLES;
    }

    const int Success = CreateProcess(
        application_path,
        command_line_args.data(),
        nullptr,
        nullptr,
        TRUE,
        CREATE_NO_WINDOW,
        nullptr,
        nullptr,
        &startup_info,
        &process_info
    );
    DWORD last_error = GetLastError();
    CloseHandle(stdout_wr);
    CloseHandle(stderr_wr);

    if (!Success)
    {
        CloseHandle(process_info.hProcess);
        CloseHandle(process_info.hThread);
        CloseHandle(stdout_rd);
        CloseHandle(stderr_rd);
        return ProcessExecutionResult::Failure(last_error);
    }

    CloseHandle(process_info.hThread);

    string std_out;
    if (stdout_rd)
    {
        stdout_thread = thread([&]
        {
            DWORD n;
            const size_t buffer_size = 1000;
            char buffer[buffer_size];
            for (;;)
            {
                n = 0;
                const int success = ReadFile(
                    stdout_rd,
                    buffer,
                    buffer_size,
                    &n,
                    nullptr
                );

                if (!success || n == 0)
                {
                    break;
                }
                string s(buffer, n);
                std_out += s;
            }
        });
    }

    uint32_t return_code;
    WaitForSingleObject(process_info.hProcess, INFINITE);
    GetExitCodeProcess(process_info.hProcess, (DWORD*)&return_code);

    CloseHandle(process_info.hProcess);

    if (stdout_thread.joinable())
    {
        stdout_thread.join();
    }

    if (stderr_thread.joinable())
    {
        stderr_thread.join();
    }

    CloseHandle(stdout_rd);
    CloseHandle(stderr_rd);

    return { std_out, return_code };
}

bool Os::IsProcessRunning(const wchar_t* process_name)
{
    bool running = false;
    PROCESSENTRY32 process_entry;
    process_entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    if (Process32First(snapshot, &process_entry))
    {
        while (Process32Next(snapshot, &process_entry))
        {
            if (!_wcsicmp(process_entry.szExeFile, process_name))
            {
                running = true;
            }
        }
    }

    CloseHandle(snapshot);
    return running;
}

bool Os::IsProcessRunningByPath(const std::wstring& process_path)
{
    PROCESSENTRY32 entry;
    entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);
    if (Process32First(snapshot, &entry))
    {
        while (Process32Next(snapshot, &entry))
        {
            wchar_t path[MAX_PATH];
            HANDLE process = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, entry.th32ProcessID);
            if (process)
            {
                GetModuleFileNameEx(process, nullptr, path, MAX_PATH);
                CloseHandle(process);
                if (process_path == path)
                {
                    CloseHandle(snapshot);
                    return true;
                }
            }
        }
    }

    CloseHandle(snapshot);
    return false;
}

string Os::GetEnvVariable(string name)
{
    char* value;
    size_t len;
    const errno_t err = _dupenv_s(&value, &len, name.c_str());
    if (err != 0)
    {
        throw WinApiErrorException(L"Failed to get environment variable " + wstring(name.begin(), name.end()) + L".",
            err);
    }

    string result = string(value);
    free(value);

    return result;
}

string Os::GetLocalAppDataPath()
{
    return GetEnvVariable("LOCALAPPDATA");
}

string Os::GetTmpFolderPath()
{
    return GetEnvVariable("TMP");
}

long Os::ChangeShortcutTarget(const wchar_t* shortcut_path, const wchar_t* target_path)
{
    HRESULT hr;
    IShellLink* psl;
    IPersistFile* ppf;

    hr = CoInitialize(nullptr);
    if (FAILED(hr)) {
        return hr;
    }

    hr = CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER, IID_IShellLink, reinterpret_cast<LPVOID*>(&psl));
    if (FAILED(hr)) {
        CoUninitialize();
        return hr;
    }

    hr = psl->QueryInterface(IID_IPersistFile, reinterpret_cast<LPVOID*>(&ppf));
    if (FAILED(hr)) {
        psl->Release();
        CoUninitialize();
        return hr;
    }

    hr = ppf->Load(shortcut_path, STGM_READWRITE);
    if (FAILED(hr)) {
        ppf->Release();
        psl->Release();
        CoUninitialize();
        return hr;
    }

    hr = psl->SetPath(target_path);
    if (FAILED(hr)) {
        ppf->Release();
        psl->Release();
        CoUninitialize();
        return hr;
    }

    hr = ppf->Save(shortcut_path, TRUE);
    if (FAILED(hr)) {
        ppf->Release();
        psl->Release();
        CoUninitialize();
        return hr;
    }

    ppf->Release();
    psl->Release();
    CoUninitialize();

    return 0;
}

void Os::RemovePinnedIcons(PCWSTR shortcut_path)
{
    HRESULT hr = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (SUCCEEDED(hr))
    {
        IShellItem* item;
        hr = SHCreateItemFromParsingName(shortcut_path, nullptr, IID_PPV_ARGS(&item));

        if (SUCCEEDED(hr))
        {
            IStartMenuPinnedList* pStartMenuPinnedList;
            hr = CoCreateInstance(CLSID_StartMenuPin, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pStartMenuPinnedList));
            if (SUCCEEDED(hr))
            {
                pStartMenuPinnedList->RemoveFromList(item);
                pStartMenuPinnedList->Release();
            }

            item->Release();
        }
    }

    CoUninitialize();
}

void Os::SetFolderPermissions(LPWSTR path, LPCWSTR security_descriptor)
{
    PSECURITY_DESCRIPTOR pSD = nullptr;
    if (ConvertStringSecurityDescriptorToSecurityDescriptorW(security_descriptor, SDDL_REVISION_1, &pSD, nullptr))
    {
        if (SetFileSecurity(path, DACL_SECURITY_INFORMATION, pSD))
        {
            LogMessage(format(L"Folder {0} permissions changed successfully.", path));
        }
        else
        {
            LogMessage(format(L"Failed to change folder {0} permissions.", path), GetLastError());
        }

        LocalFree(pSD);
    }
    else
    {
        LogMessage(format(L"Failed to convert security descriptor {0}", security_descriptor), GetLastError());
    }
}