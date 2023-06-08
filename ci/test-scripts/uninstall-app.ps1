$protonFolder = "C:\Program Files\Proton\VPN"
$protonUninstallExe = $protonFolder + "\unins000.exe"
if (Test-Path -Path $protonFolder) {
    Start-Process -FilePath $protonUninstallExe -ArgumentList "/verysilent" -Wait -ErrorAction Ignore
    Remove-Item $protonFolder -Recurse -ErrorAction Ignore
}
