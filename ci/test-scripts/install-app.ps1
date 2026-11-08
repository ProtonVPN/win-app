$installerPartialDir = $env:CI_PROJECT_DIR + "\Setup\Installers\ProtonVPN_*.exe"
$installerPath = Get-ChildItem -Path $installerPartialDir | Select-Object -First 1
$msgPath = "C:\Program Files\Proton\VPN\unins000.msg"

Write-Output ("Installer path: " + $installerPath)

$p = Start-Process -FilePath $installerPath -ArgumentList "/verysilent" -PassThru
if (-not $p.WaitForExit(240 * 1000)) {
    & taskkill.exe /PID $p.Id /T /F 2>&1 | Out-Null
    throw "Installer did not finish within 240s and was force-killed."
}

Start-Sleep -Seconds 10

if (-not (Test-Path $msgPath)) {
    throw "Install did not complete properly - missing unins000.msg"
}