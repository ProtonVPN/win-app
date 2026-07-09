@echo off

if not defined PLATFORM (
    set PLATFORM=x64
)

set currentDir=%~dp0
set publishDir=%currentDir%src\bin\win-%PLATFORM%\publish\
set publishDirBTI=%currentDir%src\bin\win-%PLATFORM%\BTI\publish\
set binDir=%currentDir%src\bin\
set resourcesDir=%binDir%Resources\
set mainDir=%binDir%
set cppDependenciesHash=8ca7d36f
set localAgentHash=3e8ad1b
set ipv6chaosHash=c3baf3a
set binaryStatusHash=c751e8c
set srpHash=e5ac410

if "%~1"=="publish" (
    set resourcesDir=%publishDir%Resources\
    set mainDir=%publishDir%
) else if "%~1"=="publish-BTI" (
	set resourcesDir=%publishDirBTI%Resources\
    set mainDir=%publishDirBTI%
) else (
    :: These dll files only need to be copied for development
    :: In CI dll files are collected and packed with the installer

    if "%PLATFORM%"=="x64" (
        xcopy %currentDir%Setup\Native\x64\wireguard.dll %binDir% /y
        xcopy %currentDir%Setup\Native\x64\tunnel.dll %binDir% /y
        xcopy %currentDir%Setup\Native\x64\wireguard-tunnel-tcp.dll %binDir% /y
        xcopy %currentDir%Setup\Native\x64\wintun.dll %binDir% /y
        xcopy %currentDir%Setup\Native\x64\libcrypto-3-x64.dll %resourcesDir% /y
        xcopy %currentDir%Setup\Native\x64\libpkcs11-helper-1.dll %resourcesDir% /y
        xcopy %currentDir%Setup\Native\x64\libssl-3-x64.dll %resourcesDir% /y
        xcopy %currentDir%Setup\Native\x64\openvpn.exe %resourcesDir% /y
        xcopy %currentDir%Setup\Native\x64\vcruntime140.dll %resourcesDir% /y
    )

    if "%PLATFORM%"=="arm64" (
        xcopy %currentDir%Setup\Native\arm64\wireguard.dll %binDir% /y
        xcopy %currentDir%Setup\Native\arm64\tunnel.dll %binDir% /y
        xcopy %currentDir%Setup\Native\arm64\wireguard-tunnel-tcp.dll %binDir% /y
        xcopy %currentDir%Setup\Native\arm64\wintun.dll %binDir% /y
        xcopy %currentDir%Setup\Native\arm64\libcrypto-3-arm64.dll %resourcesDir% /y
        xcopy %currentDir%Setup\Native\arm64\libpkcs11-helper-1.dll %resourcesDir% /y
        xcopy %currentDir%Setup\Native\arm64\libssl-3-arm64.dll %resourcesDir% /y
        xcopy %currentDir%Setup\Native\arm64\openvpn.exe %resourcesDir% /y
    )
)

if "%~2" NEQ "srponly" (
    call :FetchDependency ProtonVPN.IPFilter.dll            ip-filter/%cppDependenciesHash%          %PLATFORM% %resourcesDir% || exit /b 1
    call :FetchDependency ProtonVPN.NetworkUtil.dll         network-util/%cppDependenciesHash%       %PLATFORM% %resourcesDir% || exit /b 1
    call :FetchDependency ProtonVPN.InstallActions.x86.dll  install-actions/%cppDependenciesHash%    x86        %resourcesDir% || exit /b 1
    call :FetchDependency ProtonVPN.InstallActions.dll      install-actions/%cppDependenciesHash%    %PLATFORM% %resourcesDir% || exit /b 1
    call :FetchDependency LocalAgent.dll                    local-agent/%localAgentHash%             %PLATFORM% %resourcesDir% || exit /b 1
)

call :FetchDependency proton_vpn_ipv6chaos.dll     ipv6chaos-cffi/%ipv6chaosHash%        %PLATFORM% || exit /b 1
call :FetchDependency proton_srp_cffi.dll          srp-cffi/%srpHash%                    %PLATFORM% || exit /b 1
call :FetchDependency proton_vpn_binary_status.dll binary-status-cffi/%binaryStatusHash% %PLATFORM% || exit /b 1

echo Dependencies done %time%
exit /b 0

:FetchDependency
:: Args: %~1 file name, %~2 path prefix, %~3 platform folder, %~4 dest dir (optional)
set destDir=%~4
if not defined destDir set destDir=%mainDir%
if not exist "%destDir%" mkdir "%destDir%"
echo Fetching %~1 %time%

echo "%DEPENDENCY_CACHE_URL%/%~2/%~3/%~1"

curl --fail -o "%destDir%%~1" "%DEPENDENCY_CACHE_URL%/%~2/%~3/%~1" || (
    echo ERROR: Failed to fetch %~1 from %DEPENDENCY_CACHE_URL%/%~2/%~3/%~1 1>&2
    exit /b 1
)
echo file saved %destDir%%~1
exit /b 0