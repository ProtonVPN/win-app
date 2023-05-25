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

public class UserArrowLeft : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M10 5a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm1 0a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm-6.697 5.6a1.5 1.5 0 0 1 1.2-.6H9V9H5.503a2.5 2.5 0 0 0-2 1l-1.2 1.6c-.742.989-.036 2.4 1.2 2.4H9v-1H3.503a.5.5 0 0 1-.4-.8l1.2-1.6ZM15 11.5a.5.5 0 0 0-.5-.5h-3.793l1.147-1.146a.5.5 0 0 0-.708-.708l-1.858 1.859a.7.7 0 0 0 0 .99l1.858 1.859a.5.5 0 0 0 .708-.708L10.707 12H14.5a.5.5 0 0 0 .5-.5Z";
}