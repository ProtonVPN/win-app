$installerPartialDir = $env:CI_PROJECT_DIR + "\Setup\Installers\ProtonVPN_*.exe"
$installerPath = Get-ChildItem -Path $installerPartialDir

Write-Output ("Installer path: " + $installerPath)

Start-Process -FilePath $installerPath -ArgumentList "/verysilent" -PassThru -Wait