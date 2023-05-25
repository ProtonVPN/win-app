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

public class Mailbox : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4.5 2A4.5 4.5 0 0 0 0 6.5v6.9a.6.6 0 0 0 .6.6h14.8a.6.6 0 0 0 .6-.6V6.5A4.5 4.5 0 0 0 11.5 2h-7Zm2.829 1H11.5A3.5 3.5 0 0 1 15 6.5V13H9V6.5A4.491 4.491 0 0 0 7.329 3ZM8 13V6.5a3.5 3.5 0 1 0-7 0V13h7Zm1.5-7a.5.5 0 0 1 .5-.5h3.5a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-.5.5H12a.5.5 0 0 1-.5-.5V6.5H10a.5.5 0 0 1-.5-.5Zm3 .5v1h.5v-1h-.5Zm-10-1a.5.5 0 0 0 0 1H6a.5.5 0 0 0 0-1H2.5Z";
}