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

public class BookmarkFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2 3.4c0-.84 0-1.26.163-1.581a1.5 1.5 0 0 1 .656-.656C3.139 1 3.559 1 4.4 1h7.2c.84 0 1.26 0 1.581.163a1.5 1.5 0 0 1 .655.656c.164.32.164.74.164 1.581v9.857c0 .86 0 1.29-.17 1.49a.71.71 0 0 1-.596.247c-.26-.02-.565-.325-1.173-.933l-3.835-3.835c-.079-.079-.119-.119-.164-.133a.2.2 0 0 0-.124 0c-.045.014-.085.054-.164.133l-3.835 3.835c-.608.608-.912.912-1.173.933a.71.71 0 0 1-.596-.247c-.17-.2-.17-.63-.17-1.49V3.4Z";
}