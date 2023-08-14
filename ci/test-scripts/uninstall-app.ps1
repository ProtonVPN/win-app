$protonFolder = "C:\Program Files\Proton\VPN"
$protonUninstallExe = $protonFolder + "\unins000.exe"
if(Test-Path -Path $protonUninstallExe) {
    Start-Process -FilePath $protonUninstallExe -ArgumentList "/verysilent" -Wait -ErrorAction Ignore
}
if (Test-Path -Path $protonFolder) {
    Remove-Item $protonFolder -Recurse -ErrorAction Ignore
}
