$protonFolder = "C:\Program Files\Proton\VPN"
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1"
$processes = Get-Process | Where-Object { $_.ProcessName -like "*ProtonVPN*" }

foreach ($process in $processes) {
    Write-Host "Killing process $($process.ProcessName) (ID: $($process.Id))"
    $process.Kill()
}

if (Test-Path -Path $protonFolder) {
    Get-ChildItem -Path $protonFolder -Recurse | ForEach-Object {
        $_ | Remove-Item -Force -Recurse -ErrorAction Ignore
    }
    Remove-Item $protonFolder -Force -Recurse -ErrorAction Ignore
}

if (Test-Path $regPath) {
    Remove-Item -Path $regPath -Force
}