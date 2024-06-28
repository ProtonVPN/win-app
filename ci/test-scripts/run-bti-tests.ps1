param (
    [string]$Category
)

$output = & VSTest.Console.exe src\bin\ProtonVPN.UI.Tests.dll /Settings:ci/test-scripts/TestRun/test-run-settings.xml /TestCaseFilter:"Category=$Category"
$exitCode = $LASTEXITCODE

$keywords = @("at ", "Stack Trace", "Total", "passed", "failed", "exception", "error message")

$filteredOutput = $output | Where-Object {
    $line = $_
    $keywords | ForEach-Object { 
        if ($line -match $_) { return $true }
    }
}

$filteredOutput

exit $exitCode