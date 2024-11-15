param (
    [string]$Category
)

$output = & VSTest.Console.exe src\bin\e2e\ProtonVPN.UI.Tests.dll /Settings:.testsettings.xml /TestCaseFilter:"Category=$Category"
$exitCode = $LASTEXITCODE

$keywords = @("BVI-", "BackdropLocal", "missing frame","worldTransform", "0.00, 0.00", "chunk", "decoding stream")

#Filter noise from some of the runners
$filteredOutput = $output | Where-Object {
    $line = $_
    $shouldDrop = $false
    $keywords | ForEach-Object {
        if ($line -match $_) { 
            $shouldDrop = $true
        }
    }
	
    if ($line -match "passed" -or $line -match "failed") {
        $shouldDrop = $false
    }
    return -not $shouldDrop
}

$filteredOutput

exit $exitCode
