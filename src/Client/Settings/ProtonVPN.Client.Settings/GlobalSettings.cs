/*
 * Copyright (c) 2023 Proton AG
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

using System.Collections.Concurrent;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Dns.Contracts;

namespace ProtonVPN.Client.Settings;

public class GlobalSettings : IGlobalSettings
{
    private readonly IGlobalSettingsCache _globalCache;

    public string Language
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Unencrypted) ?? DefaultSettings.Language;
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public DeviceLocation? DeviceLocation
    {
        get => _globalCache.GetValueType<DeviceLocation>(SettingEncryption.Encrypted);
        set => _globalCache.SetValueType(value, SettingEncryption.Encrypted);
    }

    public string? UserId
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? AccessToken
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? RefreshToken
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UniqueSessionId
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UnauthAccessToken
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UnauthRefreshToken
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public string? UnauthUniqueSessionId
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Encrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public bool IsAutoLaunchEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAutoLaunchEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public AutoLaunchMode AutoLaunchMode
    {
        get => _globalCache.GetValueType<AutoLaunchMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.AutoLaunchMode;
        set => _globalCache.SetValueType<AutoLaunchMode>(value, SettingEncryption.Unencrypted);
    }

    public int[] WireGuardUdpPorts
    {
        get => _globalCache.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.WireGuardUdpPorts;
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int[] WireGuardTcpPorts
    {
        get => _globalCache.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.WireGuardTcpPorts;
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int[] WireGuardTlsPorts
    {
        get => _globalCache.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.WireGuardTlsPorts;
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int[] OpenVpnTcpPorts
    {
        get => _globalCache.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.OpenVpnTcpPorts;
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int[] OpenVpnUdpPorts
    {
        get => _globalCache.GetReferenceType<int[]>(SettingEncryption.Unencrypted) ?? DefaultSettings.OpenVpnUdpPorts;
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public ConcurrentDictionary<string, DnsResponse>? DnsCache
    {
        get => _globalCache.GetReferenceType<ConcurrentDictionary<string, DnsResponse>>(SettingEncryption.Unencrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public bool IsAlternativeRoutingEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsAlternativeRoutingEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsKillSwitchEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsKillSwitchEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsBetaAccessEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsBetaAccessEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool AreAutomaticUpdatesEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.AreAutomaticUpdatesEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public bool IsGlobalSettingsMigrationDone
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsGlobalSettingsMigrationDone;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public KillSwitchMode KillSwitchMode
    {
        get => _globalCache.GetValueType<KillSwitchMode>(SettingEncryption.Unencrypted) ?? DefaultSettings.KillSwitchMode;
        set => _globalCache.SetValueType<KillSwitchMode>(value, SettingEncryption.Unencrypted);
    }

    public List<FeatureFlag> FeatureFlags
    {
        get => _globalCache.GetListValueType<FeatureFlag>(SettingEncryption.Encrypted) ?? DefaultSettings.FeatureFlags;
        set => _globalCache.SetListValueType(value, SettingEncryption.Encrypted);
    }

    public bool IsFeatureConnectedServerCheckEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Encrypted) ?? DefaultSettings.IsFeatureConnectedServerCheckEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Encrypted);
    }

    public TimeSpan ConnectedServerCheckInterval
    {
        get => _globalCache.GetValueType<TimeSpan>(SettingEncryption.Encrypted) ?? DefaultSettings.ConnectedServerCheckInterval;
        set => _globalCache.SetValueType<TimeSpan>(value, SettingEncryption.Encrypted);
    }

    public ChangeServerSettings ChangeServerSettings
    {
        get => _globalCache.GetValueType<ChangeServerSettings>(SettingEncryption.Encrypted) ?? DefaultSettings.ChangeServerSettings;
        set => _globalCache.SetValueType<ChangeServerSettings>(value, SettingEncryption.Encrypted);
    }

    public bool IsShareCrashReportsEnabled
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsShareCrashReportsEnabled;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public string? ActiveAlternativeApiBaseUrl
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Unencrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public VpnProtocol[] DisabledSmartProtocols
    {
        get => _globalCache.GetReferenceType<VpnProtocol[]>(SettingEncryption.Encrypted) ?? [];
        set => _globalCache.SetReferenceType(value, SettingEncryption.Encrypted);
    }

    public Dictionary<string, Dictionary<string, string?>>? LegacySettingsByUsername
    {
        get => _globalCache.GetReferenceType<Dictionary<string, Dictionary<string, string?>>>(SettingEncryption.Unencrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public int TotalCountryCount
    {
        get => _globalCache.GetValueType<int>(SettingEncryption.Unencrypted) ?? DefaultSettings.TotalCountryCount;
        set => _globalCache.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public int TotalServerCount
    {
        get => _globalCache.GetValueType<int>(SettingEncryption.Unencrypted) ?? DefaultSettings.TotalServerCount;
        set => _globalCache.SetValueType<int>(value, SettingEncryption.Unencrypted);
    }

    public string? LastProcessVersionMismatchRestartVersions
    {
        get => _globalCache.GetReferenceType<string>(SettingEncryption.Unencrypted);
        set => _globalCache.SetReferenceType(value, SettingEncryption.Unencrypted);
    }

    public DateTimeOffset? LastProcessVersionMismatchRestartUtcDate
    {
        get => _globalCache.GetValueType<DateTimeOffset>(SettingEncryption.Unencrypted);
        set => _globalCache.SetValueType(value, SettingEncryption.Unencrypted);
    }

    public TimeSpan WireGuardConnectionTimeout
    {
        get => _globalCache.GetValueType<TimeSpan>(SettingEncryption.Unencrypted) ?? DefaultSettings.WireGuardConnectionTimeout;
        set => _globalCache.SetValueType<TimeSpan>(value, SettingEncryption.Unencrypted);
    }

    public bool IsEfficiencyModeAllowed
    {
        get => _globalCache.GetValueType<bool>(SettingEncryption.Unencrypted) ?? DefaultSettings.IsEfficiencyModeAllowed;
        set => _globalCache.SetValueType<bool>(value, SettingEncryption.Unencrypted);
    }

    public GlobalSettings(IGlobalSettingsCache globalSettingsCache)
    {
        _globalCache = globalSettingsCache;
    }
}