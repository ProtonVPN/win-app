$protonVpnString = "ProtonVPN"
$app = Get-WmiObject -Class Win32_Product | Where-Object {
    $_.Name -match $protonVpnString
}

$appPath = ($app | Where-Object {
    $_.Name -eq $protonVpnString
}).InstallLocation

if ($app) {
    $output = $app.Uninstall();
}

if (Test-Path $appPath) {
    Remove-Item $appPath -Recurse
}