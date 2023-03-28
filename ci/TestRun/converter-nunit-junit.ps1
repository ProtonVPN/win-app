$outputPath = "/results/" + $env:CI_JOB_NAME + "-results.xml"
$folder = Join-Path ($pwd) "/results/"
If(!(test-path -PathType container $folder))
{
      New-Item -ItemType Directory -Path $folder
}
$xml = Resolve-Path nunit-results\ProtonVPN.UI.Tests.xml
$output = Join-Path ($pwd) $outputPath
$xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
$xslt.Load(".\ci\TestRun\nunit3-junit.xslt");
$xslt.Transform($xml, $output);