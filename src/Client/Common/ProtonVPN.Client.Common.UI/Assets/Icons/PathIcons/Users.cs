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

public class Users : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M5.5 7a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Zm0 1a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Zm1.876 2a.98.98 0 0 1 .832.467l.76 1.224a.2.2 0 0 1 .005.209.178.178 0 0 1-.165.1H2.192a.178.178 0 0 1-.165-.1.197.197 0 0 1 .006-.21l.759-1.223A.98.98 0 0 1 3.624 10h3.752Zm-5.434-.06A1.98 1.98 0 0 1 3.624 9h3.752c.684 0 1.32.355 1.682.94l.76 1.224c.495.8-.075 1.836-1.01 1.836H2.192c-.935 0-1.505-1.037-1.01-1.836l.76-1.224ZM12.022 9H10v1h2.021c.43 0 .84.194 1.118.536l.79.968c.172.21.009.496-.214.496H10.98v1h2.735c1.106 0 1.668-1.296.99-2.128l-.791-.969A2.443 2.443 0 0 0 12.02 9ZM12 6a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm1 0a2 2 0 1 1-4 0 2 2 0 0 1 4 0Z";
}