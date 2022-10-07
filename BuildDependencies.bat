set outputDirX86=..\bin\x86\
set outputDirX64=..\bin\x64\
set platformToolset=v143

::ProtonVPN.IpFilter.dll (x86)
msbuild src\ProtonVPN.IpFilterLib\ProtonVPN.IpFilterLib.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=Win32 /p:Configuration=Release /p:OutDir=%outputDirX86%
msbuild src\ProtonVPN.IpFilter\ProtonVPN.IpFilter.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=Win32 /p:Configuration=Release /p:OutDir=%outputDirX86%

::ProtonVPN.IpFilter.dll (x64)
msbuild src\ProtonVPN.IpFilterLib\ProtonVPN.IpFilterLib.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=x64 /p:Configuration=Release /p:OutDir=%outputDirX64%
msbuild src\ProtonVPN.IpFilter\ProtonVPN.IpFilter.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=x64 /p:Configuration=Release /p:OutDir=%outputDirX64%

::ProtonVPN.NetworkUtil.dll (x86)
msbuild src\ProtonVPN.NetworkUtil\ProtonVPN.NetworkUtil.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=Win32 /p:Configuration=Release /p:OutDir=%outputDirX86%

::ProtonVPN.NetworkUtil.dll (x64)
msbuild src\ProtonVPN.NetworkUtil\ProtonVPN.NetworkUtil.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=x64 /p:Configuration=Release /p:OutDir=%outputDirX64%

::ProtonVPN.InstallActions.dll (x86)
msbuild src\ProtonVPN.InstallActions\ProtonVPN.InstallActions.vcxproj /p:PlatformToolset=%platformToolset% /p:Platform=Win32 /p:Configuration=Release

::GoSrp.dll
pushd %~dp0\src\srp\windows\cshared
set fn=GoSrp
set gn=main.go
set CGO_ENABLED=1

set GOARCH=386
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\x86\%fn%.dll %gn%

set GOARCH=amd64
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\x64\%fn%.dll %gn%

::LocalAgent.dll
pushd %~dp0\src\ProtonVPN.LocalAgent\localAgentWin
set GOOS=windows
set GO111MODULE=off
set CGO_CFLAGS=-O3 -Wall -Wno-unused-function -Wno-switch -std=gnu11 -DWINVER=0x0601
set CC=x86_64-w64-mingw32-gcc

set GOARCH=386
go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o "..\..\bin\Resources\32-bit\LocalAgent.dll" || exit /b 1

set GOARCH=amd64
go build -buildmode c-shared -ldflags="-w -s" -trimpath -v -o "..\..\bin\Resources\64-bit\LocalAgent.dll" || exit /b 1