@echo off

set configuration="Debug"
set platform="x64"
set outputPath=publish
set runtimeIdentifier="win-x64"
set platform=x64

if "%~1"=="Release" (
    set configuration="Release"
)

dotnet publish src\Client\ProtonVPN.Client\ProtonVPN.Client.csproj -c %configuration% -r %runtimeIdentifier% --self-contained -o %outputPath% /p:Platform=%platform%
dotnet publish src\ProtonVPN.Service\ProtonVPN.Service.csproj -c %configuration% -r %runtimeIdentifier% --self-contained -o %outputPath% /p:Platform=%platform%
dotnet publish src\ProtonVPN.TlsVerify\ProtonVPN.TlsVerify.csproj -c %configuration% -r %runtimeIdentifier% --self-contained -o %outputPath% /p:Platform=%platform%
dotnet publish src\ProtonVPN.WireGuardService\ProtonVPN.WireGuardService.csproj -c %configuration% -r %runtimeIdentifier% --self-contained -o %outputPath% /p:Platform=%platform%
dotnet publish src\ProtonVPN.RestoreInternet\ProtonVPN.RestoreInternet.csproj -c %configuration% -r %runtimeIdentifier% --self-contained -o %outputPath% /p:Platform=%platform%
dotnet publish src\ProtonVPN.Launcher\ProtonVPN.Launcher.csproj -c %configuration% -r %runtimeIdentifier% --self-contained -o %outputPath% /p:Platform=%platform%  -p:PublishSingleFile=true