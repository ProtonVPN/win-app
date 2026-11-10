/*
 * Copyright (c) 2026 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class ScriptHelper
{
    private const string ADD_FIREWALL_RULES_SCRIPT = @"
    New-NetFirewallRule -DisplayName 'Block Chrome Outbound' -Direction Outbound -Program 'C:\Program Files\Google\Chrome\Application\chrome.exe' -Action Block
    New-NetFirewallRule -DisplayName 'Block Chrome Inbound' -Direction Inbound -Program 'C:\Program Files\Google\Chrome\Application\chrome.exe' -Action Block
    ";
    private const string REMOVE_FIREWALL_RULES_SCRIPT = @"
    if (Get-NetFirewallRule -DisplayName 'Block Chrome Outbound' -ErrorAction SilentlyContinue) {
        Remove-NetFirewallRule -DisplayName 'Block Chrome Outbound'
    }
    if (Get-NetFirewallRule -DisplayName 'Block Chrome Inbound' -ErrorAction SilentlyContinue) {
        Remove-NetFirewallRule -DisplayName 'Block Chrome Inbound'
    }
    ";

    private static readonly string _netAdapter = TestEnvironment.AreTestsRunningLocally() ? "Wi-Fi" : "Ethernet";
    private static readonly string _disableInternetScript = $@"Disable-NetAdapter -Name ""{_netAdapter}"" -Confirm:$false";
    private static readonly string _enableInternetScript = $@"Enable-NetAdapter -Name ""{_netAdapter}"" -Confirm:$false";

    private const string VPN_QOS_POLICY_NAME = "LimitProtonVPN";
    private static string InstalledServicePath => Path.Combine(TestEnvironment.GetProtonClientFolder(), "ProtonVPNService.exe");
    private static string RestoreInternetExe => Path.Combine(TestEnvironment.GetProtonClientFolder(), "ProtonVPN.RestoreInternet.exe");
    private static string SetVpnLimitScript => $"New-NetQosPolicy -Name '{VPN_QOS_POLICY_NAME}' -AppPathNameMatchCondition '{InstalledServicePath}' -ThrottleRateActionBitsPerSecond 512";
    private static readonly string _removeVpnLimitScript = $@"
Remove-NetQosPolicy -Name '{VPN_QOS_POLICY_NAME}' -Confirm:$false -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
gpupdate /force | Out-Null
$LASTEXITCODE = 0
Start-Sleep -Seconds 10
$stillExists = Get-NetQosPolicy -Name '{VPN_QOS_POLICY_NAME}' -ErrorAction SilentlyContinue
if ($stillExists) {{
    throw ""QoS policy '{VPN_QOS_POLICY_NAME}' is still present after removal and gpupdate /force - VPN traffic may still be throttled to 512 bits/sec.""
}}
exit 0
";

    private static readonly string _windowsUsername = Environment.UserName;
    private const int DISCONNECT_WIRE_GUARD_ATTEMPTS = 2;
    private static readonly string _configName = "wg0";
    private static readonly string _configPath = $@"C:\Users\{_windowsUsername}\Downloads\{_configName}.conf";

    private static readonly string _connectWireGuardScript = $@"& 'C:\Program Files\WireGuard\wireguard.exe' /installtunnelservice '{_configPath}'";
    private static readonly string _checkIsWireGuardConnectedScript = "wg show";
    private static readonly string _stringToCheckInWG = "endpoint:";
    private static readonly string _disconnectWireGuardScript = $@"
$tunnelService = Get-Service -Name ""*{_configName}*"" -ErrorAction SilentlyContinue
if ($tunnelService) {{
    'WireGuard tunnel service found, status: ' + $tunnelService.Status
    $handshake = (wg show {_configName} latest-handshakes) -split '\s+' | Select-Object -Last 1
    'WireGuard latest handshake (unix time, 0 = never): ' + $handshake
    & 'C:\Program Files\WireGuard\wireguard.exe' /uninstalltunnelservice {_configName}
}}
";
    private static readonly string _checkWireGuardTunnelRemovedScript = $@"
if (Get-Service -Name '*{_configName}*' -ErrorAction SilentlyContinue) {{ 'present' }} else {{ 'absent' }}
";
    private static readonly string _removeWireGuardConfigFileScript = $@"
if (Test-Path '{_configPath}') {{
    Remove-Item -Path '{_configPath}' -Force
}}
";

    // The app's own WireGuard tunnel: the service name is the one the app installs, the adapter name is the tunnel config name.
    private const string APP_WIRE_GUARD_SERVICE_NAME = "ProtonVPN WireGuard";
    private const string APP_WIRE_GUARD_ADAPTER_NAME = "ProtonVPN";
    private const int WIRE_GUARD_LEFTOVERS_WAIT_SECONDS = 30;

    // Prints nothing when no tunnel is left over, so a clean run adds nothing to the CI job log.
    private static readonly string _removeAppWireGuardLeftoversScript = $@"
$report = @()
$appService = Get-Service -Name '{APP_WIRE_GUARD_SERVICE_NAME}' -ErrorAction SilentlyContinue
if ($appService -and $appService.Status -ne 'Stopped') {{
    $report += 'app service was ' + $appService.Status
    Stop-Service -Name '{APP_WIRE_GUARD_SERVICE_NAME}' -Force -ErrorAction SilentlyContinue
    try {{ $appService.WaitForStatus('Stopped', [TimeSpan]::FromSeconds({WIRE_GUARD_LEFTOVERS_WAIT_SECONDS})) }} catch {{ }}
    $appService.Refresh()
    $report += 'after stop: ' + $appService.Status
}}
$deadline = (Get-Date).AddSeconds({WIRE_GUARD_LEFTOVERS_WAIT_SECONDS})
do {{
    $adapters = @(Get-NetAdapter -Name '{APP_WIRE_GUARD_ADAPTER_NAME}' -ErrorAction SilentlyContinue)
    if ($adapters.Count -eq 0) {{ break }}
    Start-Sleep -Seconds 1
}} while ((Get-Date) -lt $deadline)
if ($adapters.Count -gt 0) {{
    $report += 'adapters still present: ' + (($adapters | ForEach-Object {{ $_.Name + ' (' + $_.Status + ')' }}) -join ', ')
}}
$report -join '; '
";

    // Read-only snapshot of the machine state that matters for tunnel and adapter problems. It is only saved for failed tests.
    private const string COLLECT_SYSTEM_DIAGNOSTICS_SCRIPT = @"
'=== Event logs ==='
Get-WinEvent -ListLog Application,System,Setup | Select-Object LogName,LogMode,IsLogFull,FileSize,MaximumSizeInBytes | Format-Table -AutoSize | Out-String -Width 300
'=== Services ==='
Get-Service -Name 'ProtonVPN*','WireGuard*' -ErrorAction SilentlyContinue | Select-Object Name,Status,StartType | Format-Table -AutoSize | Out-String -Width 300
sc.exe queryex 'ProtonVPN WireGuard'
'=== Network adapters ==='
Get-NetAdapter -IncludeHidden -ErrorAction SilentlyContinue | Select-Object Name,InterfaceDescription,Status | Format-Table -AutoSize | Out-String -Width 300
'=== Interfaces holding 10.2.0.2 ==='
Get-NetIPAddress -IPAddress '10.2.0.2' -ErrorAction SilentlyContinue | Select-Object InterfaceAlias,IPAddress,AddressState | Format-Table -AutoSize | Out-String -Width 300
'=== Service Control Manager warnings and errors, last 15 minutes ==='
Get-WinEvent -FilterHashtable @{LogName='System'; ProviderName='Service Control Manager'; StartTime=(Get-Date).AddMinutes(-15)} -ErrorAction SilentlyContinue | Where-Object { $_.LevelDisplayName -ne 'Information' } | Format-List TimeCreated,Id,Message | Out-String -Width 300
'=== ProtonVPN WFP filters (needs admin) ==='
$wfpFile = Join-Path $env:TEMP 'proton-wfp-filters.xml'
Remove-Item $wfpFile -ErrorAction SilentlyContinue
$wfpProcess = Start-Process -FilePath 'netsh.exe' -ArgumentList 'wfp','show','filters',('file=' + $wfpFile) -PassThru -WindowStyle Hidden
if ($wfpProcess.WaitForExit(20000)) {
    if (Test-Path $wfpFile) {
        $wfpNames = @(Select-String -Path $wfpFile -Pattern '<name>ProtonVPN[^<]*</name>' -AllMatches | ForEach-Object { $_.Matches } | ForEach-Object { $_.Value -replace '</?name>','' })
        if ($wfpNames.Count -gt 0) { $wfpNames | Group-Object | Sort-Object Name | ForEach-Object { '' + $_.Count + ' x ' + $_.Name } } else { 'no ProtonVPN filters found' }
    } else { 'no WFP dump was written (probably not running as admin)' }
} else { 'the WFP dump did not finish within 20 seconds'; $wfpProcess.Kill() }
Remove-Item $wfpFile -ErrorAction SilentlyContinue
exit 0
";

    public static string CollectSystemDiagnostics()
    {
        try
        {
            return RunPowerShellScript(COLLECT_SYSTEM_DIAGNOSTICS_SCRIPT, timeout: TestConstants.OneMinuteTimeout);
        }
        catch (Exception ex)
        {
            return $"Could not collect the system diagnostics: {ex.GetType().Name}: {ex.Message.Split('\n')[0].Trim()}";
        }
    }

    public static string RunPowerShellScript(string psScript, bool shouldEnableLogging = false, string? stringToAssert = null, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TestConstants.ThirtySecondsTimeout;

        ProcessStartInfo psi = new()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using Process process = new();
        process.StartInfo = psi;
        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        bool exited = process.WaitForExit(effectiveTimeout);

        if (!exited)
        {
            TestContext.WriteLine($"PowerShell script '{psScript}' did not exit within {effectiveTimeout.TotalSeconds}s. Killing the process tree.");
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch { }
            process.WaitForExit(TestConstants.FiveSecondsTimeout);
        }

        string psOutput = outputTask.Wait(TestConstants.FiveSecondsTimeout) ? outputTask.Result : string.Empty;
        string psError = errorTask.Wait(TestConstants.FiveSecondsTimeout) ? errorTask.Result : string.Empty;

        if (shouldEnableLogging)
        {
            TestContext.WriteLine($"PS OUTPUT: {psOutput}");
            TestContext.WriteLine($"PS ERROR: {psError}");
        }

        if (!exited)
        {
            throw new TimeoutException($"PowerShell script did not complete within {effectiveTimeout.TotalSeconds}s and was force-killed.\n" +
                $"Script: {psScript}\nPS OUTPUT: {psOutput}\nPS ERROR: {psError}");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"PowerShell script exited with code {process.ExitCode}.\n" +
                $"Script: {psScript}\nPS OUTPUT: {psOutput}\nPS ERROR: {psError}");
        }

        if (!string.IsNullOrEmpty(stringToAssert))
        {
            Assert.That(psOutput, Does.Contain(stringToAssert));
        }

        return psOutput.Trim();
    }

    public static void RestoreInternet()
    {
        RunPowerShellScript($"& '{RestoreInternetExe}'");
    }

    public static void EnableInternet()
    {
        RunPowerShellScript(_enableInternetScript);
    }

    public static void DisableInternet()
    {
        RunPowerShellScript(_disableInternetScript);
    }

    public static void AddChromeFirewallRule()
    {
        RunPowerShellScript(ADD_FIREWALL_RULES_SCRIPT);
    }

    public static void RemoveChromeFirewallRule()
    {
        RunPowerShellScript(REMOVE_FIREWALL_RULES_SCRIPT);
    }

    public static void SetVpnSpeedLimit()
    {
        RunPowerShellScript(SetVpnLimitScript);
    }

    public static void RemoveVpnSpeedLimit()
    {
        RunPowerShellScript(_removeVpnLimitScript, timeout: TestConstants.TwoMinutesTimeout);
    }

    public static void ConnectToWireGuard()
    {
        CreateWireGuardConfigFile();
        RunPowerShellScript(_connectWireGuardScript);
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        VerifyWireGuardIsConnected();
    }

    public static void DisconnectFromWireGuard()
    {
        // The script only prints something when it finds a tunnel service, so no output means there was nothing to remove.
        // Uninstalling the tunnel service can take longer than the timeout and a forced kill may leave it half-removed,
        // so retry once and then fail only if the tunnel service is really still there.
        string scriptOutput = string.Empty;
        bool timedOut = false;
        for (int attempt = 1; attempt <= DISCONNECT_WIRE_GUARD_ATTEMPTS; attempt++)
        {
            try
            {
                scriptOutput = RunPowerShellScript(_disconnectWireGuardScript, timeout: TestConstants.TwoMinutesTimeout);
                break;
            }
            catch (TimeoutException ex)
            {
                timedOut = true;
                TestContext.WriteLine($"WireGuard disconnect attempt {attempt}/{DISCONNECT_WIRE_GUARD_ATTEMPTS} timed out: {ex.Message}");
            }
        }

        if (!string.IsNullOrWhiteSpace(scriptOutput) || timedOut)
        {
            TestContext.WriteLine(scriptOutput);
            Thread.Sleep(TestConstants.TwoSecondsTimeout);
            string tunnelState = RunPowerShellScript(_checkWireGuardTunnelRemovedScript);
            if (!tunnelState.Contains("absent"))
            {
                throw new InvalidOperationException($"The WireGuard tunnel service '{_configName}' is still present after {DISCONNECT_WIRE_GUARD_ATTEMPTS} disconnect attempts.\n{scriptOutput}");
            }
        }

        RemoveWireGuardConfigFile();
    }

    // Stops the app's own WireGuard tunnel if one is left over and waits for its adapter to go away. The external wg0 tunnel is handled by DisconnectFromWireGuard.
    // It never fails the test: it logs one line only when something was left over or the check itself failed.
    public static void RemoveAppWireGuardLeftovers()
    {
        try
        {
            string report = RunPowerShellScript(_removeAppWireGuardLeftoversScript, timeout: TestConstants.TwoMinutesTimeout);
            if (!string.IsNullOrWhiteSpace(report))
            {
                TestContext.WriteLine($"[App WireGuard leftovers] {report}");
            }
        }
        catch (Exception ex)
        {
            // The exception message contains the whole script, so only its first line is logged.
            TestContext.WriteLine($"[App WireGuard leftovers] check failed: {ex.GetType().Name}: {ex.Message.Split('\n')[0].Trim()}");
        }
    }

    private static void VerifyWireGuardIsConnected()
    {
        RunPowerShellScript(_checkIsWireGuardConnectedScript, shouldEnableLogging: false, _stringToCheckInWG);
    }

    private static void CreateWireGuardConfigFile()
    {
        string decodedConfig = GetDecodedWireGuardConfig();

        //indentation needs to stay like this
        string createWireGuardConfigFileScript = $@"
$Encoding = New-Object System.Text.UTF8Encoding($false)
$config = @'
{decodedConfig}
'@
[System.IO.File]::WriteAllText('{_configPath}', $config, $Encoding)
";

        RunPowerShellScript(createWireGuardConfigFileScript);
    }

    private static void RemoveWireGuardConfigFile()
    {
        RunPowerShellScript(_removeWireGuardConfigFileScript);
    }

    private static string GetDecodedWireGuardConfig()
    {
        string wireGuardConfigBase64 = Environment.GetEnvironmentVariable("UI_TESTS_WIREGUARD_CONFIG") ?? throw new Exception("Missing UI_TESTS_WIREGUARD_CONFIG env var.");

        byte[] configBytes = Convert.FromBase64String(wireGuardConfigBase64);
        return Encoding.UTF8.GetString(configBytes);
    }
}