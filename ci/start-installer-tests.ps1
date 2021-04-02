$tasktLogsDir = $env:TASKT_LOG_PATH
Get-ChildItem -Path $tasktLogsDir -Include *.* -File -Recurse | foreach { $_.Delete()}

$projectDir = $env:CI_PROJECT_DIR
$testScriptDir = $projectDir + "\test\ProtonVPN.UI.Test\InstallerScripts\FreshInstall.xml"

Write-EventLog -LogName "Application" -Source "ProtonVPN" -EventID 6 -EntryType Information -Message $testScriptDir -Category 0