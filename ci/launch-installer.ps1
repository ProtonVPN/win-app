$version = (get-item .\src\bin\ProtonVPN.exe).VersionInfo | % {("{0}.{1}.{2}" -f $_.FileMajorPart,$_.FileMinorPart,$_.FileBuildPart)}
$hash = $env:CI_COMMIT_SHORT_SHA
$projectDir = $env:CI_PROJECT_DIR
$installerPath = $projectDir + "\Setup\ProtonVPN-SetupFiles\ProtonVPN_win_v" + $version + "-" + $hash + ".exe"

Write-Output ("Write event with installer path: " + $installerPath)

Write-EventLog -LogName "Application" -Source "ProtonVPN" -EventID 2 -EntryType Information -Message $installerPath -Category 0