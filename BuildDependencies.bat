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

set buildParams=/p:PlatformToolset=v143 /p:Configuration=Release /p:OutDir=%resourcesDir% /clp:ErrorsOnly
set x86buildParams=%buildParams% /p:Platform=Win32
set x64buildParams=%buildParams% /p:Platform=%PLATFORM%

if not defined GOSRPONLY (
    echo compiling ProtonVPN.IPFilter.dll %time%
    msbuild src\ProtonVPN.IpFilter\ProtonVPN.IpFilter.vcxproj %x64buildParams% || exit /b %ERRORLEVEL%

    echo compiling ProtonVPN.NetworkUtil.dll %time%
    msbuild src\ProtonVPN.NetworkUtil\ProtonVPN.NetworkUtil.vcxproj %x64buildParams% || exit /b %ERRORLEVEL%

    echo compiling ProtonVPN.InstallActions.x86.dll %time%
    msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj %x86buildParams% || exit /b %ERRORLEVEL%

    echo compiling ProtonVPN.InstallActions.dll %time%
    msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj %x64buildParams% || exit /b %ERRORLEVEL%

    echo compiling LocalAgent.dll %time%
    
    if "%PLATFORM%"=="x64" (
        pushd %currentDir%src\ProtonVPN.LocalAgent\localAgentWin
        set GO111MODULE=off
        set CGO_CFLAGS=-O3 -Wall -Wno-unused-function -Wno-switch -std=gnu11 -DWINVER=0x0601

        go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o %resourcesDir%LocalAgent.dll
        if %ERRORLEVEL% equ 0 (
            echo file saved %resourcesDir%LocalAgent.dll
        )
    )

    if "%PLATFORM%"=="arm64" (
        docker run --rm ^
        -e GOARCH="arm64" ^
        -e GOOS="windows" ^
        -e GO111MODULE="off" ^
        -v %currentDir%\src\ProtonVPN.LocalAgent:/go/work ^
        -w /go/work/localAgentWin x1unix/go-mingw:1.23 ^
        go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o LocalAgent.dll .
        
        xcopy %currentDir%src\ProtonVPN.LocalAgent\localAgentWin\LocalAgent.dll %resourcesDir% /y
    )
)

echo compiling GoSrp.dll %time%

if "%PLATFORM%"=="x64" (
    pushd %currentDir%src\srp\windows\cshared
    set GO111MODULE=on
    go build -buildmode=c-shared -v -ldflags="-s -w" -o %resourcesDir%GoSrp.dll main.go
    if %ERRORLEVEL% equ 0 (
        echo file saved %resourcesDir%GoSrp.dll
    )
)

if "%PLATFORM%"=="arm64" (
    docker run --rm ^
    -e GOARCH="arm64" ^
    -e GOOS="windows" ^
    -e GO111MODULE="on" ^
    -v %currentDir%\src\srp:/go/work ^
    -w /go/work/windows/cshared x1unix/go-mingw:1.23 ^
    go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o GoSrp.dll main.go
        
    xcopy %currentDir%src\srp\windows\cshared\GoSrp.dll %resourcesDir% /y
)

set ipv6chaosFileName=proton_vpn_ipv6chaos.dll

echo Fetching %ipv6chaosFileName% %time%

curl -o "%mainDir%%ipv6chaosFileName%" "%IPV6_CHAOS_DLL_PATH%/v0.0.0/%PLATFORM%/%ipv6chaosFileName%"
if %ERRORLEVEL% equ 0 (
  echo file saved %mainDir%%ipv6chaosFileName%
)

echo Dependencies done %time%