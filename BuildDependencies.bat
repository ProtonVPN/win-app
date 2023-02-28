set outputDir=..\bin\Resources\
set platformToolset=v143
set GOARCH=amd64

msbuild src\ProtonVPN.IpFilter\ProtonVPN.IpFilter.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=x64 /p:Configuration=Release /p:OutDir=%outputDir%
msbuild src\ProtonVPN.NetworkUtil\ProtonVPN.NetworkUtil.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=x64 /p:Configuration=Release /p:OutDir=%outputDir%
msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=Win32 /p:Configuration=Release
msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=x64 /p:Configuration=Release

::GoSrp.dll
pushd %~dp0\src\srp\windows\cshared
set fn=GoSrp
set gn=main.go
set CGO_ENABLED=1
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\Resources\%fn%.dll %gn%

::LocalAgent.dll
pushd %~dp0\src\ProtonVPN.LocalAgent\localAgentWin
set GOOS=windows
set GO111MODULE=off
set CGO_CFLAGS=-O3 -Wall -Wno-unused-function -Wno-switch -std=gnu11 -DWINVER=0x0601
set CC=x86_64-w64-mingw32-gcc
go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o "..\..\bin\Resources\LocalAgent.dll" || exit /b 1