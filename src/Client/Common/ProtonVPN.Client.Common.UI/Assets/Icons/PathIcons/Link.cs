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

public class Link : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M9.023 2.027a3.5 3.5 0 0 1 4.95 4.95l-2.437 2.437a.5.5 0 1 1-.707-.707l2.437-2.437A2.5 2.5 0 1 0 9.73 2.734L7.293 5.171a.5.5 0 1 1-.707-.707l2.437-2.437Zm1.098 3.852a.5.5 0 0 1 0 .707l-3.535 3.535a.5.5 0 1 1-.708-.707L9.414 5.88a.5.5 0 0 1 .707 0ZM2.03 13.97a3.5 3.5 0 0 0 4.95 0l2.44-2.439a.5.5 0 1 0-.708-.707l-2.44 2.44a2.5 2.5 0 1 1-3.535-3.536l2.44-2.44a.5.5 0 0 0-.707-.707l-2.44 2.44a3.5 3.5 0 0 0 0 4.95Z";
}