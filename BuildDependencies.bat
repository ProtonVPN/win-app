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
pushd src\srp\windows\cshared
set fn=GoSrp
set gn=main.go
set CGO_ENABLED=1

set GOARCH=386
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\x86\%fn%.dll %gn%

set GOARCH=amd64
go build -buildmode=c-shared -v -ldflags="-s -w" -o ..\..\..\bin\x64\%fn%.dll %gn%