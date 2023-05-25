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

public class UsersPlus : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7.5 7a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Zm0 1a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Zm-1.876 2h3.752a.98.98 0 0 1 .833.467l.758 1.224a.197.197 0 0 1 .006.209.178.178 0 0 1-.165.1H4.192a.178.178 0 0 1-.165-.1.197.197 0 0 1 .006-.21l.759-1.223A.98.98 0 0 1 5.624 10Zm-1.682-.06A1.98 1.98 0 0 1 5.624 9h3.752c.684 0 1.32.355 1.682.94l.76 1.224c.495.8-.075 1.836-1.01 1.836H4.192c-.935 0-1.505-1.037-1.01-1.836l.76-1.224ZM13.036 9H12v1h1.036c.424 0 .83.194 1.108.535l.786.968c.174.214.004.497-.21.497h-1.746v1h1.745c1.107 0 1.66-1.3.987-2.127l-.786-.968A2.428 2.428 0 0 0 13.036 9Zm.464-3a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm1 0a2 2 0 1 1-4 0 2 2 0 0 1 4 0ZM2 7V5.5a.5.5 0 0 1 1 0V7h1.5a.5.5 0 0 1 0 1H3v1.5a.5.5 0 0 1-1 0V8H.5a.5.5 0 0 1 0-1H2Z";
}