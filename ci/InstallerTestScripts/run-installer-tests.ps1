$installerAbstractPath = "\\DESKTOP-48DK8LA\Shared\Installers\" + $args[0] + "\*.exe"
$installerPath = Get-ChildItem $installerAbstractPath
$tasktLogsDir = $env:TASKT_LOG_PATH

function Log-Message
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$LogMessage
    )
    $dateNow = Get-Date
    Write-Host ("{0} [INFO] - {1}" -f ($dateNow.toUniversalTime()), $LogMessage)
}

function Wait-ForProcess
{
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [string]$name,
        [int]$timeout
    )

    $timer = [Diagnostics.Stopwatch]::StartNew()
    
    while (((Get-Process -Name $name -ErrorAction SilentlyContinue).Count -eq 0) -and ($timer.Elapsed.TotalSeconds -lt $timeout)) {
        Start-Sleep -Seconds 1
    }
}

Log-Message 'Clearing environment...'
Stop-Process -name "Proton*" -Force
Stop-Process -name "msiexec" -ErrorAction SilentlyContinue -Force
Stop-Process -name "taskt*" -ErrorAction SilentlyContinue -Force

Log-Message 'Deleting ProtonVPN Client...'
Get-Package 'ProtonVPN' -ErrorAction SilentlyContinue | Uninstall-Package | Out-Null

Log-Message 'Starting installer...'
Write-EventLog -LogName Application -Source "ProtonVPNService" -EventId 7 -EntryType Information -Message $installerPath -Category 0
Wait-ForProcess "ProtonVPN*" -timeout 180

Log-Message 'Deleting taskt log files...'
Get-ChildItem -Path $tasktLogsDir -Include *.* -File -Recurse | foreach { $_.Delete()}

Log-Message 'Starting installation tests...'
Write-EventLog -LogName Application -Source "ProtonVPNService" -EventId 6 -EntryType Information -Message "Installer script" -Category 0

\\DESKTOP-48DK8LA\Shared\TasktTestRunner.exe 
Log-Message 'Reading tests output...'

return $LASTEXITCODE