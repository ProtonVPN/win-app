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

using System.Runtime.Serialization;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public enum VpnErrorTypeContract
    {
        [EnumMember]
        None,

        [EnumMember]
        NoneKeepEnabledKillSwitch,

        [EnumMember]
        NetshError,

        [EnumMember]
        AuthorizationError,

        [EnumMember]
        TapAdapterInUseError,

        [EnumMember]
        NoTapAdaptersError,

        [EnumMember]
        TapRequiresUpdateError,

        [EnumMember]
        TlsError,

        [EnumMember]
        TlsCertificateError,

        [EnumMember]
        PingTimeoutError,

        [EnumMember]
        AdapterTimeoutError,

        [EnumMember]
        ClientKeyMismatch,

        [EnumMember]
        UserTierTooLowError,

        [EnumMember]
        Unpaid,

        [EnumMember]
        SessionLimitReached,

        [EnumMember]
        PasswordChanged,

        [EnumMember]
        ServerOffline,

        [EnumMember]
        ServerRemoved,

        [EnumMember]
        NoServers,

        [EnumMember]
        RpcServerUnavailable,

        [EnumMember]
        Unknown,

        [EnumMember]
        MissingServerPublicKey,

        [EnumMember]
        IncorrectVpnConfig,

        [EnumMember]
        ServerUnreachable,

        [EnumMember]
        GuestSession = 86100,

        [EnumMember]
        CertificateExpired = 86101,

        [EnumMember]
        CertificateRevoked = 86102,

        [EnumMember]
        SessionKilledDueToMultipleKeys = 86103,

        [EnumMember]
        UnableToVerifyCert = 86104,

        [EnumMember]
        CertRevokedOrExpired = 86105,

        [EnumMember]
        CertificateNotYetProvided = 86106,

        [EnumMember]
        SessionLimitReachedFree = 86111,

        [EnumMember]
        SessionLimitReachedBasic = 86112,

        [EnumMember]
        SessionLimitReachedPlus = 86113,

        [EnumMember]
        SessionLimitReachedVisionary = 86114,

        [EnumMember]
        SessionLimitReachedPro = 86115,

        [EnumMember]
        SessionLimitReachedUnknown = 86110,

        [EnumMember]
        PlanNeedsToBeUpgraded = 86151,
    }
}