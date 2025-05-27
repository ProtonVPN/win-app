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

namespace ProtonVPN.Client.Logic.Connection.Contracts.Enums;

public enum VpnError
{
    None,
    NoneKeepEnabledKillSwitch,
    NetshError,

    TapAdapterInUseError,
    NoTapAdaptersError,
    TapRequiresUpdateError,

    TlsError,
    TlsCertificateError,
    PingTimeoutError,
    Unpaid,
    ServerOffline,
    ServerRemoved,
    NoServers,
    Unknown,
    RpcServerUnavailable,
    ServerUnreachable,
    AdapterTimeoutError,
    ClientKeyMismatch,
    WireGuardAdapterInUseError,
    ServerValidationError,
    NoServerValidationPublicKey,
    MissingConnectionCertificate,
    BaseFilteringEngineServiceNotRunning,
    InterfaceHasForwardingEnabled,

    CertificateExpired = 86101,
    CertificateRevoked = 86102,
    SessionKilledDueToMultipleKeys = 86103,
    UnableToVerifyCert = 86104,
    CertCARevokedOrExpired = 86105,
    CertificateNotYetProvided = 86106,
    SessionBeingInstalled = 86107,
    SystemErrorOnTheServer = 86150,

    SessionLimitReachedFree = 86111,
    SessionLimitReachedBasic = 86112,
    SessionLimitReachedPlus = 86113,
    SessionLimitReachedVisionary = 86114,
    SessionLimitReachedPro = 86115,
    SessionLimitReachedUnknown = 86110,

    PlanNeedsToBeUpgraded = 86151,

    ServerSessionDoesNotMatch = 86202,
    ServerSessionError = 86203,
}