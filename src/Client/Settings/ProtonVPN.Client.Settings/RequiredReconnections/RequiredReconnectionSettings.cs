using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;

namespace ProtonVPN.Client.Settings.RequiredReconnections;

public class RequiredReconnectionSettings : IRequiredReconnectionSettings
{
    private readonly Dictionary<string, Func<bool>> _settings;

    public RequiredReconnectionSettings(
        IConnectionManager connectionManager,
        ISettings settings)
    {
        _settings = new()
        {
            {nameof(ISettings.IsSplitTunnelingEnabled), () => true},
            {nameof(ISettings.SplitTunnelingMode), () => settings.IsSplitTunnelingEnabled},
            {nameof(ISettings.SplitTunnelingStandardAppsList), () => false},
            {nameof(ISettings.SplitTunnelingInverseAppsList), () => false},
            {nameof(ISettings.SplitTunnelingStandardIpAddressesList), () => false},
            {nameof(ISettings.SplitTunnelingInverseIpAddressesList), () => false},

            {nameof(ISettings.VpnProtocol), () => true},
            {nameof(ISettings.OpenVpnAdapter), () => true},
            {nameof(ISettings.IsIpv6LeakProtectionEnabled), () => true},
            {nameof(ISettings.IsIpv6Enabled), () => true},
            {nameof(ISettings.IsLocalAreaNetworkAccessEnabled), () => settings.IsLocalDnsEnabled},
            {nameof(ISettings.IsLocalDnsEnabled), () => true},
            {nameof(ISettings.IsCustomDnsServersEnabled), () => true},
            {nameof(ISettings.CustomDnsServersList), () => settings.IsCustomDnsServersEnabled},
            {nameof(ISettings.IsPortForwardingEnabled), () =>
                !settings.IsPortForwardingEnabled &&
                connectionManager.IsConnected &&
                connectionManager.CurrentConnectionIntent?.IsPortForwardingSupported() == true &&
                connectionManager.CurrentConnectionDetails?.IsP2P != true
            },
        };
    }

    public bool IsReconnectionRequired(string settingName)
    {
        return _settings.ContainsKey(settingName) && _settings[settingName]();
    }
}
