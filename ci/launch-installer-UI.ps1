$version = (get-item .\src\bin\ProtonVPN.exe).VersionInfo | % {("{0}.{1}.{2}" -f $_.FileMajorPart,$_.FileMinorPart,$_.FileBuildPart)}
$projectDir = $env:CI_PROJECT_DIR
$installerPath = $projectDir + "\Setup\Installers\ProtonVPN_v" + $version + ".exe"

Write-EventLog -LogName "Application" -Source "ProtonVPN" -EventID 5 -EntryType Information -Message $installerPath -Category 0