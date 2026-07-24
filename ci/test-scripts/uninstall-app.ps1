$protonFolder = "C:\Program Files\Proton\VPN"
$protonUninstallExe = $protonFolder + "\unins000.exe"

function Main
{
    # Sometimes the callout driver is stuck running
    Stop-VpnCalloutService

    if (Test-Path -Path $protonFolder) {
        if (Test-Path -Path "$protonFolder\unins000.msg") {
            Start-Process -FilePath $protonUninstallExe -ArgumentList "/verysilent" -Wait -ErrorAction Ignore
            Stop-VpnCalloutService
        } else {
            Write-Warning "unins000.msg missing - installer/uninstaller left an inconsistent state, skipping uninstaller and forcing cleanup"
        }
    }

    Get-Process | Where-Object { $_.Name -like "proton*" -or $_.Name -like "unins0*" } | Stop-Process -Force -ErrorAction Ignore
    Start-Sleep -Seconds 1
    Remove-Item $protonFolder -Recurse -Force -ErrorAction Ignore

    # If the uninstaller failed for any reason, clean the registry manually
    $uninstallKey = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Proton VPN_is1"
    if (Test-Path $uninstallKey) {
        Remove-Item $uninstallKey -Recurse -Force -ErrorAction Ignore
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