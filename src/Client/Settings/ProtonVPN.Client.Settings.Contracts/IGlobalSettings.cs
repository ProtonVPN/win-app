using System.Collections.Concurrent;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Client.Settings.Contracts;

public interface IGlobalSettings
{
    string Language { get; set; }
    DeviceLocation? DeviceLocation { get; set; }
    string? UserId { get; set; }
    string? AccessToken { get; set; }
    string? RefreshToken { get; set; }
    string? UniqueSessionId { get; set; }
    string? UnauthAccessToken { get; set; }
    string? UnauthRefreshToken { get; set; }
    string? UnauthUniqueSessionId { get; set; }
    bool IsAutoLaunchEnabled { get; set; }
    AutoLaunchMode AutoLaunchMode { get; set; }
    int[] WireGuardUdpPorts { get; set; }
    int[] WireGuardTcpPorts { get; set; }
    int[] WireGuardTlsPorts { get; set; }
    int[] OpenVpnTcpPorts { get; set; }
    int[] OpenVpnUdpPorts { get; set; }
    ConcurrentDictionary<string, DnsResponse>? DnsCache { get; set; } // VPNWIN-2098 - Move to its own file
    bool IsAlternativeRoutingEnabled { get; set; }
    bool IsKillSwitchEnabled { get; set; }
    bool IsBetaAccessEnabled { get; set; }
    bool AreAutomaticUpdatesEnabled { get; set; }
    string? SkippedUpdateVersion { get; set; }
    bool IsGlobalSettingsMigrationDone { get; set; }
    KillSwitchMode KillSwitchMode { get; set; }
    List<FeatureFlag> FeatureFlags { get; set; }
    bool IsFeatureConnectedServerCheckEnabled { get; set; }
    TimeSpan ConnectedServerCheckInterval { get; set; }
    ChangeServerSettings ChangeServerSettings { get; set; }
    bool IsShareCrashReportsEnabled { get; set; }
    string? ActiveAlternativeApiBaseUrl { get; set; }
    VpnProtocol[] DisabledSmartProtocols { get; set; }
    int TotalCountryCount { get; set; }
    int TotalServerCount { get; set; }
    string? LastProcessVersionMismatchRestartVersions { get; set; }
    DateTimeOffset? LastProcessVersionMismatchRestartUtcDate { get; set; }
    TimeSpan WireGuardConnectionTimeout { get; set; }
    bool IsEfficiencyModeAllowed { get; set; }

    Dictionary<string, Dictionary<string, string?>>? LegacySettingsByUsername { get; set; }
}
