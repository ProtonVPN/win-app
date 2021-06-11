$virtualMachinesPath = Path-Join -Path $env:CI_PROJECT_DIR -ChildPath "\ci\InstallerTestScripts\virtual-machines.ps1"
. $virtualMachinesPath

Foreach ($name in $virtualMachines) {
    Write-Host("========================================== " + $name + " =========================================")
    Write-Host("Starting " + $name + " Virtual Machine...")
    Start-VM -name $name
    Write-Host "Waiting until machine starts..."
    Wait-VM -name $name
    Start-Sleep -Seconds 60
    Write-Host ("Restarting virtual machine " + $name + "...")
    Restart-VM $name -Force -Wait
    Write-Host "Waiting until Anti-Viruses updates their database (5 minutes)" 
    Start-Sleep -Seconds 300
    Write-Host "Saving virtual machine"
    Save-VM -name $name
}

