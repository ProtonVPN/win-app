@echo off

set currentDir=%~dp0
set publishDir=%currentDir%publish\
set binDir=%currentDir%src\bin\
set resourcesDir=%binDir%Resources

if "%~1"=="publish" (
    set resourcesDir=%publishDir%Resources
)

set buildParams=/p:PlatformToolset=v143 /p:Configuration=Release /p:OutDir=%resourcesDir% /clp:ErrorsOnly
set x86buildParams=%buildParams% /p:Platform=Win32
set x64buildParams=%buildParams% /p:Platform=x64

if "%~2" NEQ "gosrponly" (
    echo compiling ProtonVPN.IPFilter.dll
    msbuild src\ProtonVPN.IpFilter\ProtonVPN.IpFilter.vcxproj %x64buildParams% || exit /b %ERRORLEVEL%

    echo compiling ProtonVPN.NetworkUtil.dll
    msbuild src\ProtonVPN.NetworkUtil\ProtonVPN.NetworkUtil.vcxproj %x64buildParams% || exit /b %ERRORLEVEL%

    echo compiling ProtonVPN.InstallActions.x86.dll
    msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj %x86buildParams% || exit /b %ERRORLEVEL%

    echo compiling ProtonVPN.InstallActions.dll
    msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj %x64buildParams% || exit /b %ERRORLEVEL%

    echo compiling LocalAgent.dll
    pushd %currentDir%src\ProtonVPN.LocalAgent\localAgentWin
    set GO111MODULE=off
    set CGO_CFLAGS=-O3 -Wall -Wno-unused-function -Wno-switch -std=gnu11 -DWINVER=0x0601

    go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o %resourcesDir%\LocalAgent.dll
    if %ERRORLEVEL% equ 0 (
        echo file saved %resourcesDir%\LocalAgent.dll
    )
)

echo compiling GoSrp.dll
pushd %currentDir%src\srp\windows\cshared
set GO111MODULE=on
go build -buildmode=c-shared -v -ldflags="-s -w" -o %resourcesDir%\GoSrp.dll main.go
if %ERRORLEVEL% equ 0 (
    echo file saved %resourcesDir%\GoSrp.dll
)