
$hash = Get-FileHash -Path $setupFile -Algorithm MD5
Write-Host $hash.Hash

$newVersionBlockString  = @"
{
	"Version": "$($version)",
	"File": {
		"Url":  "https://protonvpn.com/download/ProtonVPN_win_v$($version).exe",
		"CheckSum":  "$($hash.Hash)",
		"Arguments":  "/qb"
	},
	"ChangeLog": [
		"Introduced an option to choose where your app will be minimized to after autostart (taskbar/systray).",
		"Added log size limit per file (500kb).",
		"Renamed Czech Republic to Czechia.",
		"Compression inline was removed from the template in order to remove confusion. For those who didn’t know - we have removed compression on all of our OpenVPN servers due to recent vulnerability. You can read more at: https://protonvpn.com/blog/voracle-attack/",
		"Squashed a couple of bugs."
	]
}
"@

$newVersionBlock = ConvertFrom-Json($newVersionBlockString)
$json = (Get-Content win-update.json) | Out-String | ConvertFrom-Json
$stable = $json.Categories | where { $_.Name -eq "Stable" } | Select -index 0
$stable.Releases += $newVersionBlock
$stable | ConvertTo-Json -depth 5 | Set-Content json.txt
