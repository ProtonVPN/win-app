$installerPartialDir = $env:CI_PROJECT_DIR + "\Setup\Installers\ProtonVPN_*.exe"
$installerPath = Get-ChildItem -Path $installerPartialDir
$msgPath = "C:\Program Files\Proton\VPN\unins000.msg"

Write-Output ("Installer path: " + $installerPath)

Start-Process -FilePath $installerPath -ArgumentList "/verysilent" -PassThru -Wait

Start-Sleep -Seconds 10

if (-not (Test-Path $msgPath)) {
    throw "Install did not complete properly - missing unins000.msg"
}