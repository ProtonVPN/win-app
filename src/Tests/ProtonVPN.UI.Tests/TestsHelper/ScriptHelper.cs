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
using System.Text;
using System.Threading;

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
    private static readonly string _installedServicePath = Path.Combine(TestEnvironment.GetProtonClientFolder(), "ProtonVPNService.exe");
    private static readonly string _restoreInternetExe = Path.Combine(TestEnvironment.GetProtonClientFolder(), "ProtonVPN.RestoreInternet.exe");
    private static readonly string _setVpnLimitScript = $"New-NetQosPolicy -Name '{VPN_QOS_POLICY_NAME}' -AppPathNameMatchCondition '{_installedServicePath}' -ThrottleRateActionBitsPerSecond 512";
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
    private static readonly string _configName = "wg0";
    private static readonly string _configPath = $@"C:\Users\{_windowsUsername}\Downloads\{_configName}.conf";

    private static readonly string _connectWireGuardScript = $@"& 'C:\Program Files\WireGuard\wireguard.exe' /installtunnelservice '{_configPath}'";
    private static readonly string _checkIsWireGuardConnectedScript = "wg show";
    private static readonly string _stringToCheckInWG = "endpoint:";
    private static readonly string _disconnectWireGuardScript = $@"
$tunnelService = Get-Service -Name ""*{_configName}*"" -ErrorAction SilentlyContinue
if ($tunnelService) {{
    & 'C:\Program Files\WireGuard\wireguard.exe' /uninstalltunnelservice {_configName}
}}
";
    private static readonly string _removeWireGuardConfigFileScript = $@"
if (Test-Path '{_configPath}') {{
    Remove-Item -Path '{_configPath}' -Force
}}
";

    public static void RestoreInternet()
    {
        WindowsUtils.RunPowerShellScript($"& '{_restoreInternetExe}'");
    }

    public static void EnableInternet()
    {
        WindowsUtils.RunPowerShellScript(_enableInternetScript);
    }

    public static void DisableInternet()
    {
        WindowsUtils.RunPowerShellScript(_disableInternetScript);
    }

    public static void AddChromeFirewallRule()
    {
        WindowsUtils.RunPowerShellScript(ADD_FIREWALL_RULES_SCRIPT);
    }

    public static void RemoveChromeFirewallRule()
    {
        WindowsUtils.RunPowerShellScript(REMOVE_FIREWALL_RULES_SCRIPT);
    }

    public static void SetVpnSpeedLimit()
    {
        WindowsUtils.RunPowerShellScript(_setVpnLimitScript);
    }

    public static void RemoveVpnSpeedLimit()
    {
        WindowsUtils.RunPowerShellScript(_removeVpnLimitScript);
    }

    public static void ConnectToWireGuard()
    {
        CreateWireGuardConfigFile();
        WindowsUtils.RunPowerShellScript(_connectWireGuardScript);
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        VerifyWireGuardIsConnected();
    }

    public static void DisconnectFromWireGuard()
    {
        WindowsUtils.RunPowerShellScript(_disconnectWireGuardScript);
        RemoveWireGuardConfigFile();
    }

    private static void VerifyWireGuardIsConnected()
    {
        WindowsUtils.RunPowerShellScript(_checkIsWireGuardConnectedScript, shouldEnableLogging: false, _stringToCheckInWG);
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

        WindowsUtils.RunPowerShellScript(createWireGuardConfigFileScript);
    }

    private static void RemoveWireGuardConfigFile()
    {
        WindowsUtils.RunPowerShellScript(_removeWireGuardConfigFileScript);
    }

    private static string GetDecodedWireGuardConfig()
    {
        string wireGuardConfigBase64 = Environment.GetEnvironmentVariable("UI_TESTS_WIREGUARD_CONFIG") ?? throw new Exception("Missing UI_TESTS_WIREGUARD_CONFIG env var.");

        byte[] configBytes = Convert.FromBase64String(wireGuardConfigBase64);
        return Encoding.UTF8.GetString(configBytes);
    }
}