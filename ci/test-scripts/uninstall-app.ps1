$protonFolder = "C:\Program Files\Proton\VPN"
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1"
$protonDataFolder = "C:\ProgramData\ProtonVPN"
$processes = Get-Process | Where-Object { $_.ProcessName -like "*ProtonVPN*" }
$userProfileFolder = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::UserProfile)
$protonLocalFolder = Join-Path -Path $userProfileFolder -ChildPath "AppData\Local\Proton\Proton VPN"

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

if (Test-Path -Path $protonLocalFolder) {
    Remove-Item -Path $protonLocalFolder -Force -Recurse -ErrorAction Ignore
}

if (Test-Path -Path $protonDataFolder) {
    Remove-Item -Path $protonDataFolder -Force -Recurse -ErrorAction Ignore
}
