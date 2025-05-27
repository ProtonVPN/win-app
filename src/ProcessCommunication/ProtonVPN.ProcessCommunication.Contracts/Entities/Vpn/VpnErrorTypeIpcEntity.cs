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

namespace ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn
{
    [DataContract]
    public enum VpnErrorTypeIpcEntity
    {
        [EnumMember]
        None,

        [EnumMember]
        NoneKeepEnabledKillSwitch,

        [EnumMember]
        NetshError,

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
        Unpaid,

        [EnumMember]
        ServerOffline,

        [EnumMember]
        ServerRemoved,

        [EnumMember]
        NoServers,

        [EnumMember]
        Unknown,

        [EnumMember]
        RpcServerUnavailable,

        [EnumMember]
        ServerUnreachable,

        [EnumMember]
        AdapterTimeoutError,

        [EnumMember]
        ClientKeyMismatch,

        [EnumMember]
        WireGuardAdapterInUseError,

        [EnumMember]
        ServerValidationError,

        [EnumMember]
        NoServerValidationPublicKey,

        [EnumMember]
        MissingConnectionCertificate,

        [EnumMember]
        BaseFilteringEngineServiceNotRunning,

        [EnumMember]
        InterfaceHasForwardingEnabled,

        [EnumMember]
        CertificateExpired = 86101,

        [EnumMember]
        CertificateRevoked = 86102,

        [EnumMember]
        SessionKilledDueToMultipleKeys = 86103,

        [EnumMember]
        UnableToVerifyCert = 86104,

        [EnumMember]
        CertCARevokedOrExpired = 86105,

        [EnumMember]
        CertificateNotYetProvided = 86106,

        [EnumMember]
        SessionBeingInstalled = 86107,

        [EnumMember]
        SystemErrorOnTheServer = 86150,

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

        [EnumMember]
        ServerSessionDoesNotMatch = 86202,

        [EnumMember]
        ServerSessionError = 86203,
    }
}