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

public class UsersFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 5.5a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0ZM3.624 9a1.98 1.98 0 0 0-1.682.94l-.76 1.224c-.495.8.075 1.836 1.01 1.836h6.616c.935 0 1.505-1.037 1.01-1.836l-.76-1.224A1.98 1.98 0 0 0 7.376 9H3.624Zm9.976.8 1.2 1.6A1 1 0 0 1 14 13h-3v-1.333a2 2 0 0 0-.4-1.2L9.5 9H12a2 2 0 0 1 1.6.8ZM11 8a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";

    protected override string IconGeometry20 { get; }
        = "M10 6.875a3.125 3.125 0 1 1-6.25 0 3.125 3.125 0 0 1 6.25 0ZM4.53 11.25c-.855 0-1.65.444-2.103 1.175l-.948 1.53c-.62 1 .093 2.295 1.261 2.295h8.27c1.168 0 1.88-1.296 1.261-2.295l-.948-1.53A2.475 2.475 0 0 0 9.22 11.25H4.53Zm12.47 1 1.5 2c.618.824.03 2-1 2h-3.75v-1.667a2.5 2.5 0 0 0-.5-1.5l-1.375-1.833H15a2.5 2.5 0 0 1 2 1ZM13.75 10a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 8.25a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM5.436 13.5a2.97 2.97 0 0 0-2.524 1.41l-1.138 1.836c-.743 1.199.112 2.754 1.514 2.754h9.924c1.402 0 2.257-1.555 1.514-2.754l-1.138-1.836a2.97 2.97 0 0 0-2.524-1.41H5.436ZM20.4 14.7l1.8 2.4c.742.989.036 2.4-1.2 2.4h-4.5v-2a3 3 0 0 0-.6-1.8l-1.65-2.2H18a3 3 0 0 1 2.4 1.2ZM16.5 12a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 11a5 5 0 1 1-10 0 5 5 0 0 1 10 0Zm-8.752 7a3.96 3.96 0 0 0-3.365 1.88l-1.517 2.448C1.375 23.927 2.515 26 4.384 26h13.232c1.87 0 3.01-2.073 2.018-3.672l-1.517-2.448A3.96 3.96 0 0 0 14.752 18H7.248ZM27.2 19.6l2.4 3.2c.989 1.319.048 3.2-1.6 3.2h-6v-2.667a4 4 0 0 0-.8-2.4L19 18h5a4 4 0 0 1 3.2 1.6ZM22 16a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z";
}