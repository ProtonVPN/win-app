$protonVpnString = "ProtonVPN"

$app = Get-WmiObject -Class Win32_Product | Where-Object {
    $_.Name -match $protonVpnString
}

if ($app) {
    $appPath = ($app | Where-Object {
        $_.Name -eq $protonVpnString
    }).InstallLocation

    $output = $app.Uninstall();

    if (Test-Path $appPath) {
        Remove-Item $appPath -Recurse
    }
}
