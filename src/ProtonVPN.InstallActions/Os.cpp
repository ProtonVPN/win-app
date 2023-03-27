#include <string>
#include <fstream>
#include <iostream>
#include <windows.h>
#include <cstdint>
#include <string>
#include <thread>
#include <tlhelp32.h>

#include "ProcessExecutionResult.h"
#include "Os.h"

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
    if (!GetExitCodeProcess(process_info.hProcess, (DWORD*)&return_code))
    {
        return_code = -1;
    }

    CloseHandle(process_info.hProcess);

    if (stdout_thread.joinable())
        stdout_thread.join();

    if (stderr_thread.joinable())
        stderr_thread.join();

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
