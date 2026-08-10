param (
    [string]$Category
)
$env:CATEGORY = $Category

New-Item -ItemType Directory -Force -Path $env:UI_TEST_REPORT_PATH | Out-Null

if ($Category -notin @("SLI", "SLI-BTI-PROD")) {
    $installerPath = Get-ChildItem -Path "$env:CI_PROJECT_DIR\Setup\Installers\ProtonVPN_*.exe" | Select-Object -First 1
    if ($installerPath -match "ProtonVPN_v(\d+\.\d+\.\d+)_") {
        $matches[1] | Out-File "$env:UI_TEST_REPORT_PATH\version.txt" -Encoding utf8
    }
}

$reportPath = "$env:UI_TEST_REPORT_PATH/results_$Category.xml"

$filter = "Category=$Category"
if ($Category -ne "5") {
    $filter += "&Category!=5"
}

$keywords = @("BVI-", "BackdropLocal", "missing frame", "worldTransform", "0.00, 0.00", "chunk", "decoding stream", "Sequence file will not be generated", "Test execution complete")

$hardTimeoutSeconds = 3120 

function Stop-LingeringProcesses {
    Get-Process | Where-Object {
        $_.Name -like "testhost*" -or $_.Name -like "vstest*" -or
        $_.Name -like "proton*" -or $_.Name -like "unins0*"
    } | Stop-Process -Force -ErrorAction Ignore
}

$stdOutFile = "$env:UI_TEST_REPORT_PATH\stdout_$Category.log"
$stdErrFile = "$env:UI_TEST_REPORT_PATH\stderr_$Category.log"
Remove-Item $stdOutFile, $stdErrFile -ErrorAction Ignore

$vstestArgs = @(
    "src\bin\e2e\ProtonVPN.UI.Tests.dll"
    "/Settings:.testsettings.xml"
    "/TestCaseFilter:$filter"
    "/Logger:junit;LogFilePath=$reportPath"
)

$process = Start-Process -FilePath "VSTest.Console.exe" -ArgumentList $vstestArgs `
    -RedirectStandardOutput $stdOutFile -RedirectStandardError $stdErrFile `
    -WorkingDirectory (Get-Location).Path -PassThru -NoNewWindow

$stopwatch = [Diagnostics.Stopwatch]::StartNew()
$linesShown = 0
$aborted = $false

function Write-NewLines {
    param([string]$FilePath, [ref]$ShownCount)

    if (-not (Test-Path $FilePath)) { return }

    $allLines = @(Get-Content -Path $FilePath -ErrorAction SilentlyContinue)
    if ($allLines.Count -le $ShownCount.Value) { return }

    $newLines = $allLines[$ShownCount.Value..($allLines.Count - 1)]
    foreach ($line in $newLines) {
        $shouldDrop = $false
        foreach ($keyword in $keywords) {
            if ($line -match $keyword) { $shouldDrop = $true; break }
        }
        if (-not $shouldDrop) { Write-Host $line }

        if ($line -match "Test Run Aborted" -or $line -match "test run timeout of") {
            $script:aborted = $true
        }
    }
    $ShownCount.Value = $allLines.Count
}

while (-not $process.HasExited) {
    Start-Sleep -Seconds 1

    Write-NewLines -FilePath $stdOutFile -ShownCount ([ref]$linesShown)

    if ($aborted -or $stopwatch.Elapsed.TotalSeconds -ge $hardTimeoutSeconds) {
        if ($aborted) {
            Write-Host "Test Run Aborted"
        }
        else {
            Write-Host "Hard timeout of $hardTimeoutSeconds seconds reached with no clean abort"
        }

        Start-Sleep -Seconds 2
        Stop-LingeringProcesses
        if (-not $process.HasExited) {
            $process.Kill()
        }
        exit 1
    }
}

Write-NewLines -FilePath $stdOutFile -ShownCount ([ref]$linesShown)

exit $process.ExitCode