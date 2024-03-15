$internalReleaseUrl = $env:INTERNAL_RELEASE_URL

$response = Invoke-RestMethod -Uri $internalReleaseUrl
$installersFolderPath = "C:\Installers"

if (-not (Test-Path -Path $installersFolderPath -PathType Container)) {
    New-Item -Path $installersFolderPath -ItemType Directory
}

$allReleases = @()

foreach ($category in $response.Categories) {
    foreach ($release in $category.Releases) {
        $allReleases += $release
    }
}

$sortedReleases = $allReleases | Sort-Object { [version]::Parse($_.Version) } -Descending

$mostRecentVersion = $sortedReleases[0]
$mostRecentVersionUrl = $mostRecentVersion.File.Url
$executableName = [System.IO.Path]::GetFileName($mostRecentVersionUrl)

$executablePath = Join-Path $installersFolderPath $executableName

if (-not (Test-Path -Path $executablePath)) {
    Invoke-WebRequest -Uri $mostRecentVersionUrl -OutFile $executablePath
}

Start-Process -FilePath $executablePath -ArgumentList "/verysilent" -PassThru -Wait