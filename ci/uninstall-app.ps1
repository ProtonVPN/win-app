if (Test-Path -Path "C:\Program Files\Proton\VPN") {
    Start-Process -FilePath "C:\Program Files\Proton\VPN\unins000.exe" -ArgumentList "/verysilent" -ErrorAction SilentlyContinue
}
