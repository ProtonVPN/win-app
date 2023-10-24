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

using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Configurations.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultTlsPinningConfigurationFactory
{
    public static ITlsPinningConfiguration Create()
    {
        return new TlsPinningConfiguration()
        {
            Enforce = true,
            PinnedDomains = new List<ITlsPinnedDomain>
            {
                new TlsPinnedDomain()
                {
                    Name = "vpn-api.proton.me",
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "CT56BhOTmj5ZIPgb/xD5mH8rY3BLo/MlhP7oPyJUEDo=", // Current.
                        "35Dx28/uzN3LeltkCBQ8RHK0tlNSa2kCpCRGNp34Gxc=", // Hot backup
                        "qYIukVc63DEITct8sFT7ebIq5qsWmuscaIKeJx+5J5A=", // Cold backup.
                    },
                },
                new TlsPinnedDomain()
                {
                    Name = "protonvpn.com",
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "+0dMG0qG2Ga+dNE8uktwMm7dv6RFEXwBoBjQ43GqsQ0=",
                        "8joiNBdqaYiQpKskgtkJsqRxF7zN0C0aqfi8DacknnI=",
                        "JMI8yrbc6jB1FYGyyWRLFTmDNgIszrNEMGlgy972e7w=",
                        "Iu44zU84EOCZ9vx/vz67/MRVrxF1IO4i4NIa8ETwiIY=",
                    },
                },
                new TlsPinnedDomain()
                {
                    Name = "[InternalReleaseHost]", // This is replaced by a CI script
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "C4SMuz+h4+fTsxOKLXRKqrR9rAzk9bknu+hlC4QYmh0=",
                    },
                },
                new TlsPinnedDomain()
                {
                    Name = Constants.ALTERNATIVE_ROUTING_HOSTNAME,
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "EU6TS9MO0L/GsDHvVc9D5fChYLNy5JdGYpJw0ccgetM=",
                        "iKPIHPnDNqdkvOnTClQ8zQAIKG0XavaPkcEo0LBAABA=",
                        "MSlVrBCdL0hKyczvgYVSRNm88RicyY04Q2y5qrBt0xA=",
                        "C2UxW0T1Ckl9s+8cXfjXxlEqwAfPM4HiW2y3UdtBeCw=",
                    },
                },
            },
        };
    }
}