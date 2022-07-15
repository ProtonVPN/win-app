/*
 * Copyright (c) 2022 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software:
 you can redistribute it and/or modify
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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;

namespace ProtonVPN.IntegrationTests.Api.Responses
{
    public class AuthInfoResponseMock : AuthInfoResponse
    {
        public AuthInfoResponseMock()
        {
            Code = ResponseCodes.OkResponse;
            Modulus =
                "-----BEGIN PGP SIGNED MESSAGE-----\n" +
                "Hash: SHA256\n\nm6V3I9fusOFsGnMtI+hVhjmN77YH6H+TeI5yaTvj/ehpLukt9qnuqy/enr0H/dOeq5ck1n8yqDsj9RvC6Xttzz1nzn10qmB2pqB4tzpEB5ybUwWwsZa3MGj0ZIaJwtOk4RUYGEpv4pd11FhscDv0pNknOVio7N7px6+oHiBwyQxDSZSbWCZYHUOJNu9bITp0C/F2hr9Vxkycyuz0E8tZQA0K7olWkIDfkLO1gVIDf/HJmJujhAxbblH7XtRpnzbm67Q3H6lulgMXCJ40M72zdCF1bYdlCrzkJApk0cpbb8L64LMF86hdRp6gJ4tpMQ/F6C76q04kvSki5RnbsFY9hQ==\n" +
                "-----BEGIN PGP SIGNATURE-----\n" +
                "Version: ProtonMail\nComment: https://protonmail.com\n\n" +
                "wl4EARYIABAFAlwB1j4JEDUFhcTpUY8mAAAGAgD/VZqGL2bmoen9c8zyVQ6z\n" +
                "+JZB607BqbvvIce4j5+5zQcA/AkNAuO4oVBV90jJYR6LVyi8bV6hf5PXsh9Y\n" +
                "DqkEgo0C\n" +
                "=87Gf\n" +
                "-----END PGP SIGNATURE-----\n";
            Salt = "yKlc5/CvObfoiw==";
            ServerEphemeral = "bOoybtQ9viUsUoloxY50uFLgr1lIc62p72nmERdyZP3jvV+sGAju8GoVGci+blvppvks+cU0dISMh+hls4EV0rX8f77r7ayIRl4SBCrD9VGCShuP/HeiujjPeGUXDFmHgXsThyqbsQ7XAIB9diQTYuyZscgr0LTYcOLZLWZkq6iEcw6vKDUJlUZVk1kAcwrja0/Zpnu30NRifcJ6iCo3r6qrySLvti+81HTWzsPwLh6aA1ClHWOrsWF/2rYSW6B9zrSgzBx5Bk7DTmSn8d7h8bKFiabbk66PSSX+28ZxemIxAjtGh3ucz6kP9gOQLrXlHVvLqBVEELJtjaOIqdDjAQ==";
            SrpSession = "b61bba30700435da67a989b244e5f7df";
            Version = 4;
        }
    }
}