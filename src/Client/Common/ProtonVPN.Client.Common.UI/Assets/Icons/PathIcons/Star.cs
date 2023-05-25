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

public class Star : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "m8 2.943-1.475 2.63c-.23.411-.633.69-1.087.773l-2.973.541 2.06 2.119c.328.337.488.806.425 1.28l-.39 2.924 2.777-1.282c.42-.194.906-.194 1.326 0l2.777 1.282-.39-2.925a1.543 1.543 0 0 1 .425-1.28l2.06-2.118-2.973-.541a1.567 1.567 0 0 1-1.087-.773L8 2.943Zm.498-1.155a.576.576 0 0 0-.996 0l-1.85 3.296a.567.567 0 0 1-.393.278l-3.795.69a.548.548 0 0 0-.308.923l2.652 2.728c.117.12.173.286.15.45l-.496 3.723c-.058.433.4.757.806.57l3.489-1.61a.582.582 0 0 1 .486 0l3.489 1.61c.406.187.864-.137.806-.57l-.497-3.723a.542.542 0 0 1 .15-.45l2.653-2.728a.548.548 0 0 0-.308-.922l-3.795-.691a.567.567 0 0 1-.394-.278L8.498 1.788Z";
}