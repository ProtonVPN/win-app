param (
    [string]$Category
)

New-Item -ItemType Directory -Force -Path $env:UI_TEST_REPORT_PATH | Out-Null

if ($Category -notin @("SLI", "SLI-BTI-PROD")) {
    $installerPath = Get-ChildItem -Path "$env:CI_PROJECT_DIR\Setup\Installers\ProtonVPN_*.exe" | Select-Object -First 1
    if ($installerPath -match "ProtonVPN_v(\d+\.\d+\.\d+)_") {
        $matches[1] | Out-File "$env:UI_TEST_REPORT_PATH\version.txt" -Encoding utf8
    }
}

$reportPath = "$env:UI_TEST_REPORT_PATH\results_$Category.xml"

$keywords = @("BVI-", "BackdropLocal", "missing frame","worldTransform", "0.00, 0.00", "chunk", "decoding stream")

& VSTest.Console.exe src\bin\e2e\ProtonVPN.UI.Tests.dll /Settings:.testsettings.xml /TestCaseFilter:"Category=$Category" /Logger:"junit;LogFilePath=$reportPath" |
ForEach-Object {
    $line = $_
    $shouldDrop = $false
    foreach ($keyword in $keywords) {
        if ($line -match $keyword) { 
            $shouldDrop = $true 
        }
    }
    if (-not $shouldDrop) {
        Write-Host $line 
    }
}

exit $LASTEXITCODE