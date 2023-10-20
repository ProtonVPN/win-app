$response = Invoke-RestMethod -Uri "https://protonvpn.com/download/windows-releases.json"
$installersFolderPath = "C:\Installers"

if (-not (Test-Path -Path $installersFolderPath -PathType Container)) {
    New-Item -Path $installersFolderPath -ItemType Directory
}

$mostRecentStableVersion = $response.Categories | Where-Object { $_.Name -eq "Stable" } | 
    Select-Object -ExpandProperty Releases | 
    Sort-Object { [version]::Parse($_.Version) } -Descending | 
    Select-Object -First 1

$mostRecentStableVersionUrl = $mostRecentStableVersion.File.Url
$executableName = [System.IO.Path]::GetFileName($mostRecentStableVersionUrl)

$executablePath = Join-Path $installersFolderPath $executableName

if (-not (Test-Path -Path $executablePath)) {
    Invoke-WebRequest -Uri $mostRecentStableVersionUrl -OutFile $executablePath
}

Start-Process -FilePath $executablePath -ArgumentList "/verysilent" -PassThru -Wait