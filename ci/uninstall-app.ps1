$app = Get-WmiObject -Class Win32_Product | Where-Object {
    $_.Name -match "ProtonVPN"   
}

if($app){
    $output = $app.Uninstall();
}