$virtualMachinesPath = Join-Path -Path $env:CI_PROJECT_DIR -ChildPath "\ci\InstallerTestScripts\virtual-machines.ps1"
. $virtualMachinesPath

$testStatus = @{}
$installerSharedFolderPath = "C:/Shared/Installers/" + $env:CI_PROJECT_ID
$installerPath = $env:CI_PROJECT_DIR + "/setup/ProtonVPN-SetupFiles/*.exe"
$testScriptsSharedPath = "C:/Shared/Scripts/"
$testScriptsPath = $env:CI_PROJECT_DIR + "/test/ProtonVPN.UI.Test/InstallerScripts/FreshInstall.xml"
$testRunnerScript = $env:CI_PROJECT_DIR + "/ci/InstallerTestScripts/run-installer-tests.ps1"

function Log-Message
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$LogMessage
    )
    $dateNow = Get-Date
    Write-Output ("{0} [INFO] - {1}" -f ($dateNow.toUniversalTime()), $LogMessage)
}

Log-Message "Moving installer to shared folder..."
New-Item $installerSharedFolderPath -itemtype Directory -ErrorAction SilentlyContinue
Get-ChildItem -Path $installerSharedFolderPath -Include *.* -File -Recurse | foreach{ $_.Delete()}
Get-ChildItem -Path $testScriptsSharedPath -Include *.* -File -Recurse | foreach{ $_.Delete()}
Copy-Item $installerPath -Destination $installerSharedFolderPath
Copy-Item $testScriptsPath -Destination $testScriptsSharedPath

Foreach ($name in $virtualMachines) {
    Write-Host("========================================== " + $name + " =========================================")
    $pWord = ConvertTo-SecureString -String $name -AsPlainText -Force
    $credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $name, $pWord
    Log-Message("Starting " + $name + " Virtual Machine...")
    Start-VM -name $name

    Log-Message "Waiting until machine starts..."
    Wait-VM -name $name

    Log-Message "Connecting to Virtual Machine..."
    Enter-PSSession -VMName $name -Credential $credential

    $output = Invoke-Command -VMName $name -Credential $credential -FilePath $testRunnerScript -ArgumentList $env:CI_PROJECT_ID
    $exitCode = $output[$output.Count - 1]
    if ($exitCode -eq 0) {
        $testStatus[$name] = "Passed"
    } else {
        Foreach ($line in $output) {
            $line
        }
        $testStatus[$name] = "Failed"
    }
    Exit-PSSession

    Log-Message "Saving virtual machine..."
    Save-VM -name $name
}

Write-Host("`n`n============== Results ==============")
$testStatus

Foreach ($name in $virtualMachines) {
    if ($testStatus[$name] -eq "Failed") {
        exit 1
    }
}
exit 0