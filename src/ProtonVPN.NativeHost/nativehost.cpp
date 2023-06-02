#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include "inc/coreclr_delegates.h"
#include <filesystem>
#include <iostream>

#include "inc/hostfxr.h"
#include "inc/nethost.h"
#include <string>
#include <shellapi.h>
#include <string>

hostfxr_close_fn close_fptr;
hostfxr_initialize_for_dotnet_command_line_fn command_line_fptr;
hostfxr_run_app_fn run_app_fptr;
hostfxr_set_runtime_property_value_fn hostfxr_set_runtime_property_value;
bool load_hostfxr(get_hostfxr_parameters params);

std::wstring GetVersionFolderPath()
{
    WCHAR buffer[MAX_PATH];
    GetModuleFileNameW(nullptr, buffer, MAX_PATH);
    const std::wstring::size_type pos = std::wstring(buffer).find_last_of(L"\\/");
    return std::wstring(buffer).substr(0, pos);
}

int WinMain(HINSTANCE hInstance,
            HINSTANCE hPrevInstance,
            LPSTR lpCmdLine,
            int cmdShow)
{
    std::filesystem::path version_folder_path(GetVersionFolderPath());
    std::filesystem::path dll_path = version_folder_path / std::filesystem::path("ProtonVPN.dll");
    std::filesystem::path host_path = version_folder_path / std::filesystem::path("ProtonVPN.exe");

    WCHAR base_directory[MAX_PATH];
    lstrcpyW(base_directory, version_folder_path.parent_path().c_str());

    SetCurrentDirectoryW(base_directory);

    get_hostfxr_parameters hostfxr_params;
    hostfxr_params.size = sizeof hostfxr_params;
    hostfxr_params.assembly_path = dll_path.c_str();
    hostfxr_params.dotnet_root = nullptr;

    if (!load_hostfxr(hostfxr_params))
    {
        assert(false && "Failure: load_hostfxr()");
        return EXIT_FAILURE;
    }

    int total_arguments = 0;
    LPWSTR* szArglist = CommandLineToArgvW(GetCommandLineW(), &total_arguments);
    const char_t** argv = static_cast<const char_t**>(calloc(total_arguments, sizeof(const char_t*)));
    argv[0] = dll_path.c_str();

    for (int i = 1; i < total_arguments; i++)
    {
        argv[i] = szArglist[i];
    }

    hostfxr_initialize_parameters params;
    params.size = sizeof params;
    params.host_path = host_path.c_str();
    params.dotnet_root = version_folder_path.c_str();

    hostfxr_handle host_context_handle;
    command_line_fptr(
        total_arguments,
        argv,
        &params,
        &host_context_handle);

    hostfxr_set_runtime_property_value(host_context_handle, L"APP_CONTEXT_BASE_DIRECTORY", base_directory);

    run_app_fptr(host_context_handle);
    close_fptr(host_context_handle);
    free(argv);

    return 0;
}

void* load_library(const char_t* path)
{
    HMODULE h = ::LoadLibraryW(path);
    assert(h != nullptr);
    return h;
}

void* get_export(void* h, const char* name)
{
    const auto address = GetProcAddress(static_cast<HMODULE>(h), name);
    assert(address != nullptr);
    return address;
}

bool load_hostfxr(get_hostfxr_parameters params)
{
    char_t buffer[MAX_PATH];
    size_t buffer_size = sizeof buffer / sizeof(char_t);

    int rc = get_hostfxr_path(buffer, &buffer_size, &params);
    if (rc != 0)
    {
        return false;
    }

    void* lib = load_library(buffer);
    command_line_fptr = (hostfxr_initialize_for_dotnet_command_line_fn)get_export(
        lib, "hostfxr_initialize_for_dotnet_command_line");
    run_app_fptr = (hostfxr_run_app_fn)get_export(lib, "hostfxr_run_app");
    hostfxr_set_runtime_property_value = (hostfxr_set_runtime_property_value_fn)get_export(
        lib, "hostfxr_set_runtime_property_value");
    close_fptr = (hostfxr_close_fn)get_export(lib, "hostfxr_close");

    return command_line_fptr &&
        run_app_fptr &&
        hostfxr_set_runtime_property_value &&
        close_fptr;
}