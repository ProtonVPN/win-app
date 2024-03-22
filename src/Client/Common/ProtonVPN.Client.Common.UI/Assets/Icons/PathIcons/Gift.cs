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

using ProtonVPN.Client.Common.UI.Assets.Icons.Base;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class Gift : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7 3.5A1.5 1.5 0 1 0 5.5 5H7V3.5Zm-4 0c0 .563.186 1.082.5 1.5H1a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1v5a2 2 0 0 0 2 2h9a2 2 0 0 0 2-2V9a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1h-2.5a2.5 2.5 0 0 0-4-3A2.5 2.5 0 0 0 3 3.5ZM1 6h5.99v2H1V6Zm5.99 3H2v5a1 1 0 0 0 1 1h3.99V9Zm1 6V9H13v5a1 1 0 0 1-1 1H7.99Zm0-7V6H14v2H7.99ZM11 3.5A1.5 1.5 0 0 1 9.5 5H8V3.5a1.5 1.5 0 1 1 3 0Z";

    protected override string IconGeometry20 { get; }
        = "M9.375 4.375A1.875 1.875 0 1 0 7.5 6.25h1.875V4.375Zm-5 0c0 .704.232 1.353.625 1.875H2.5c-.69 0-1.25.56-1.25 1.25V10c0 .69.56 1.25 1.25 1.25v5a2.5 2.5 0 0 0 2.5 2.5h10a2.5 2.5 0 0 0 2.5-2.5v-5c.69 0 1.25-.56 1.25-1.25V7.5c0-.69-.56-1.25-1.25-1.25H15a3.125 3.125 0 0 0-5-3.75 3.125 3.125 0 0 0-5.625 1.875ZM2.5 7.5h6.863V10H2.5V7.5Zm6.863 3.75H3.75v5c0 .69.56 1.25 1.25 1.25h4.363v-6.25Zm1.25 6.25v-6.25h5.637v5c0 .69-.56 1.25-1.25 1.25h-4.387Zm0-7.5V7.5H17.5V10h-6.887Zm3.762-5.625c0 1.036-.84 1.875-1.875 1.875h-1.875V4.375a1.875 1.875 0 0 1 3.75 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.25 5.25A2.25 2.25 0 1 0 9 7.5h2.25V5.25Zm-6 0c0 .844.279 1.623.75 2.25H3A1.5 1.5 0 0 0 1.5 9v3A1.5 1.5 0 0 0 3 13.5v6a3 3 0 0 0 3 3h12a3 3 0 0 0 3-3v-6a1.5 1.5 0 0 0 1.5-1.5V9A1.5 1.5 0 0 0 21 7.5h-3A3.75 3.75 0 0 0 12 3a3.75 3.75 0 0 0-6.75 2.25ZM3 9h8.236v3H3V9Zm8.236 4.5H4.5v6A1.5 1.5 0 0 0 6 21h5.236v-7.5Zm1.5 7.5v-7.5H19.5v6A1.5 1.5 0 0 1 18 21h-5.264Zm0-9V9H21v3h-8.264Zm4.514-6.75A2.25 2.25 0 0 1 15 7.5h-2.25V5.25a2.25 2.25 0 0 1 4.5 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M15 7a3 3 0 1 0-3 3h3V7ZM7 7c0 1.126.372 2.164 1 3H4a2 2 0 0 0-2 2v4a2 2 0 0 0 2 2v8a4 4 0 0 0 4 4h16a4 4 0 0 0 4-4v-8a2 2 0 0 0 2-2v-4a2 2 0 0 0-2-2h-4a5 5 0 0 0-8-6 5 5 0 0 0-9 3Zm-3 5h10.98v4H4v-4Zm10.98 6H6v8a2 2 0 0 0 2 2h6.98V18Zm2 10V18H26v8a2 2 0 0 1-2 2h-7.02Zm0-12v-4H28v4H16.98ZM23 7a3 3 0 0 1-3 3h-3V7a3 3 0 1 1 6 0Z";
}