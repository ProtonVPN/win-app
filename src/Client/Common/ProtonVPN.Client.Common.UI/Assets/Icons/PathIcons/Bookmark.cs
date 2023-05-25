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

public class Bookmark : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M11.6 2H4.4c-.437 0-.704 0-.904.017a1.281 1.281 0 0 0-.215.034.5.5 0 0 0-.23.23 1.281 1.281 0 0 0-.034.215c-.016.2-.017.467-.017.904v10.185l.232-.231 3.835-3.835.007-.008c.031-.03.089-.088.147-.138a1.2 1.2 0 0 1 1.557 0 2.509 2.509 0 0 1 .155.146l3.835 3.835.231.23.001-.326V3.4c0-.437 0-.704-.017-.904a1.29 1.29 0 0 0-.034-.215l-.004-.008a.5.5 0 0 0-.226-.222 1.281 1.281 0 0 0-.215-.034A12.83 12.83 0 0 0 11.6 2Zm-9.437-.181C2 2.139 2 2.559 2 3.4v9.857c0 .86 0 1.29.17 1.49a.71.71 0 0 0 .596.247c.26-.02.565-.325 1.173-.933l3.835-3.835c.079-.079.119-.119.164-.133a.2.2 0 0 1 .124 0c.045.014.085.054.164.133l3.835 3.835c.608.608.912.912 1.173.933a.71.71 0 0 0 .596-.247c.17-.2.17-.63.17-1.49V3.4c0-.84 0-1.26-.164-1.581a1.5 1.5 0 0 0-.655-.656C12.861 1 12.441 1 11.6 1H4.4c-.84 0-1.26 0-1.581.163a1.5 1.5 0 0 0-.656.656Z";
}