$protonFolder = "C:\Program Files\Proton\VPN"
$protonUninstallExe = Join-Path $protonFolder "unins000.exe"
$uninstallTimeoutSeconds = 120

function Main
{
    Stop-ProtonClientProcesses
    Stop-VpnCalloutService

    if (Test-Path -Path $protonFolder) {
        if (Test-Path -Path "$protonFolder\unins000.msg") {
            Invoke-BoundedUninstall
            Stop-VpnCalloutService
        } else {
            Write-Warning "unins000.msg missing - installer/uninstaller left an inconsistent state, skipping uninstaller and forcing cleanup"
        }
    }

    Stop-ProtonClientProcesses
    Start-Sleep -Seconds 1
    Remove-Item $protonFolder -Recurse -Force -ErrorAction Ignore

    # If the uninstaller failed for any reason, clean the registry manually
    $uninstallKey = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1"
    if (Test-Path $uninstallKey) {
        Remove-Item $uninstallKey -Recurse -Force -ErrorAction Ignore
    }
}

function Invoke-BoundedUninstall {
    $p = Start-Process -FilePath $protonUninstallExe `
        -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES") -PassThru

    if (-not $p.WaitForExit($uninstallTimeoutSeconds * 1000)) {
        Write-Warning "Uninstaller did not exit within ${uninstallTimeoutSeconds}s - killing it and forcing cleanup."
        & taskkill.exe /PID $p.Id /T /F 2>&1 | Out-Null
    }

    $deadline = (Get-Date).AddSeconds($uninstallTimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $helpers = Get-Process -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like "_iu*" -or $_.Name -like "*unins*" }
        if (-not $helpers) { return }
        Start-Sleep -Seconds 2
    }

    Write-Warning "Inno uninstall helper still running after ${uninstallTimeoutSeconds}s - killing it."
    Stop-ProtonClientProcesses
}

function Stop-ProtonClientProcesses {
    Get-Process -ErrorAction SilentlyContinue | Where-Object {
        $_.Name -like "ProtonVPN*" -or
        $_.Name -like "*unins*" -or
        $_.Name -like "_iu*"
    } | ForEach-Object {
        Write-Host "Killing leftover process $($_.Name) (PID $($_.Id))"
        & taskkill.exe /PID $_.Id /T /F 2>&1 | Out-Null
    }
}

function Stop-VpnCalloutService {
    $MaxRetries = 3
    $RetryDelaySeconds = 2
    $TimeoutSeconds = 10

    try {
        $protonService = Get-Service -Name "ProtonVPNCallout" -ErrorAction Ignore

        if ($null -eq $protonService) {
            Write-Host "ProtonVPNCallout service not found - skipping"
            return
        }

        for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
            $protonService.Refresh()

            if ($protonService.Status -eq "Stopped") {
                Write-Host "ProtonVPNCallout stopped successfully on attempt $attempt"
                return
            }

            Write-Warning "Attempt $attempt/$MaxRetries ProtonVPNCallout still running - stopping now"

            $protonService | Stop-Service -Force -ErrorAction Ignore

            try {
                $protonService.WaitForStatus("Stopped", [TimeSpan]::FromSeconds($TimeoutSeconds))
            }
            catch {
                Write-Warning "Timeout waiting for service to stop on attempt $attempt"

                if ($attempt -lt $MaxRetries) {
                    Write-Host "Waiting ${RetryDelaySeconds}s before retry..."
                    Start-Sleep -Seconds $RetryDelaySeconds
                    $protonService.Refresh()
                }
            }
        }

        $protonService.Refresh()
        if ($protonService.Status -ne "Stopped") {
            Write-Warning "ProtonVPNCallout failed to stop after $MaxRetries attempts"
        }
    }
    catch {
        Write-Warning "ProtonVPNCallout service failed to stop: $($_.Exception.Message)"
    }
}

Main