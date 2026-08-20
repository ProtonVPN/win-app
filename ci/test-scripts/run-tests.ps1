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

$hardTimeoutSeconds = 2700 # 45 min

function Stop-ProcessWithTimeout {
    param(
        [Parameter(Mandatory)][int]$ProcessId,
        [int]$TimeoutSeconds = 15
    )

    $job = Start-Job -ScriptBlock {
        param($targetId)
        try { Stop-Process -Id $targetId -Force -ErrorAction Stop } catch {}
    } -ArgumentList $ProcessId

    $done = Wait-Job $job -Timeout $TimeoutSeconds
    $result = $true
    if (-not $done) {
        Write-Host "WARNING: Stop-Process on PID $ProcessId did not return within ${TimeoutSeconds}s - likely stuck in a driver/kernel call."
        $result = $false
    }
    Remove-Job $job -Force -ErrorAction Ignore
    return $result
}

function Stop-LingeringProcesses {
    $targets = Get-Process | Where-Object {
        $_.Name -like "WerFault*" -or $_.Name -like "testhost*" -or $_.Name -like "vstest*" -or
        $_.Name -like "proton*" -or $_.Name -like "unins0*"
    }

    foreach ($p in $targets) {
        Write-Host "Stopping lingering process: $($p.Name) (PID $($p.Id))"
        Stop-ProcessWithTimeout -ProcessId $p.Id -TimeoutSeconds 10 | Out-Null
    }
}

function Stop-DriverWithTimeout {
    param(
        [Parameter(Mandatory)][string]$DriverName,
        [int]$TimeoutSeconds = 15
    )

    $job = Start-Job -ScriptBlock {
        param($name)
        & sc.exe stop $name | Out-Null
    } -ArgumentList $DriverName

    $done = Wait-Job $job -Timeout $TimeoutSeconds
    if (-not $done) {
        Write-Host "WARNING: 'sc.exe stop $DriverName' did not return within ${TimeoutSeconds}s - driver is likely wedged and cannot be cleanly stopped from user mode. A reboot of the runner may be required to fully clear it."
    }
    Remove-Job $job -Force -ErrorAction Ignore
    return $done
}

function Stop-StuckServices {
    $drv = "ProtonVPNCallout"

    try {
        $status = (Get-Service -Name $drv -ErrorAction Stop).Status
        if ($status -ne 'Stopped') {
            Write-Host "Attempting to stop driver/service: $drv (was $status)"
            Stop-DriverWithTimeout -DriverName $drv -TimeoutSeconds 15 | Out-Null
        }
        else {
            Write-Host "$drv already stopped"
        }
    }
    catch {
        Write-Host "Could not query '$drv' (may not exist): $_"
    }
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
$script:aborted = $false
$timedOut = $false

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

function Test-RunWasSuccessful {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        Write-Host "WARNING: output file '$FilePath' not found - treating run as failed."
        return $false
    }

    $content = Get-Content -Path $FilePath -ErrorAction SilentlyContinue
    $wasSuccessful = $true

    if ($content | Select-String -Pattern "Test Run Aborted" -Quiet) {
        Write-Host "WARNING: 'Test Run Aborted' found in output."
        $wasSuccessful = $false
    }

    if ($content | Select-String -Pattern "test run timeout of" -Quiet) {
        Write-Host "WARNING: 'test run timeout of' found in output."
        $wasSuccessful = $false
    }

    $failedLine = $content | Select-String -Pattern '^\s*Failed:\s*(\d+)' | Select-Object -Last 1
    if ($failedLine -and [int]$failedLine.Matches[0].Groups[1].Value -gt 0) {
        $wasSuccessful = $false
    }

    $blameLines = $content | Select-String -Pattern "Data collector 'Blame' message"
    if ($blameLines) {
        $badBlameLines = $blameLines | Where-Object { $_.Line -notmatch 'All tests finished running' }
        if ($badBlameLines) {
            Write-Host "WARNING: Blame data collector reported a problem: $($badBlameLines[0].Line.Trim())"
            $wasSuccessful = $false
        }
    }

    $totalLine = $content | Select-String -Pattern '^\s*Total tests:\s*(\d+|Unknown)' | Select-Object -Last 1
    if (-not $totalLine -or $totalLine.Matches[0].Groups[1].Value -eq 'Unknown') {
        $secondaryReasons += "summary shows 'Total tests: Unknown' - run may not have finished normally."
        $wasSuccessful = $false
    }

    foreach ($reason in $primaryReasons) {
        Write-Host "WARNING: $reason"
    }
    foreach ($reason in $secondaryReasons) {
        Write-Host "WARNING: $reason"
    }

    return $wasSuccessful
}

try {
    while (-not $process.HasExited) {
        Start-Sleep -Seconds 1
        Write-NewLines -FilePath $stdOutFile -ShownCount ([ref]$linesShown)

        if ($script:aborted -or $stopwatch.Elapsed.TotalSeconds -ge $hardTimeoutSeconds) {
            $timedOut = $true

            if ($script:aborted) {
                Write-Host "Test Run Aborted"
            }
            else {
                Write-Host "Hard timeout of $hardTimeoutSeconds seconds reached with no clean abort"
            }

            Write-Host "Attempting bounded termination of VSTest (PID $($process.Id)) and its process tree..."

            if (-not $process.HasExited) {
                $killedOk = Stop-ProcessWithTimeout -ProcessId $process.Id -TimeoutSeconds 15
                if (-not $killedOk) {
                    Write-Host "Primary process did not respond to kill within timeout - it is likely stuck in a kernel/driver call. Not waiting further; proceeding to cleanup so the job can still finish."
                }
            }

            break
        }
    }

    if (-not $timedOut) {
        $process.WaitForExit()
        Write-NewLines -FilePath $stdOutFile -ShownCount ([ref]$linesShown)

        if (Test-RunWasSuccessful -FilePath $stdOutFile) {
            exit 0
        }

        Write-Host "WARNING: VSTest exit code was: $($process.ExitCode)"
        exit 1
    }
    else {
        Write-NewLines -FilePath $stdOutFile -ShownCount ([ref]$linesShown)
        Write-Host "Job is exiting with failure due to timeout/abort. Check $reportPath and $stdOutFile for the last test that ran before the stall."
        exit 1
    }
}
finally {
    Write-Host "Running cleanup..."

    Stop-LingeringProcesses
    Stop-StuckServices

    Start-Sleep -Milliseconds 500
    Write-Host "Cleanup finished."
}