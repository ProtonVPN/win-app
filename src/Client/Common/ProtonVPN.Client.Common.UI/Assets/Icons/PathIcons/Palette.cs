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

public class Palette : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6 9a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm0-2a1 1 0 1 0 0-2 1 1 0 0 0 0 2Zm4-2a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm2 2a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z M1 8.5a7.5 7.5 0 1 1 15 0c0 .291-.017.538-.051.766-.176 1.152-1.31 1.734-2.284 1.734H9.387a.887.887 0 0 0-.887.887v.031c0 .509.254.984.677 1.266l.165.11A1.477 1.477 0 0 1 8.522 16H8.5A7.5 7.5 0 0 1 1 8.5ZM8.5 2a6.5 6.5 0 0 0 0 13h.023a.477.477 0 0 0 .265-.874l-.165-.11A2.521 2.521 0 0 1 7.5 11.918v-.031C7.5 10.845 8.344 10 9.386 10h4.279c.683 0 1.22-.398 1.295-.885A3.98 3.98 0 0 0 15 8.5 6.5 6.5 0 0 0 8.5 2Z";

    protected override string IconGeometry20 { get; }
        = "M1.875 10a8.125 8.125 0 1 1 16.203.877c-.073.68-.723 1.212-1.572 1.212h-5.63c-2.245 0-3.352 2.732-1.74 4.295l1.03.999c.18.175.207.384.162.525a.302.302 0 0 1-.104.154.36.36 0 0 1-.224.063A8.125 8.125 0 0 1 1.875 10ZM10 .625a9.375 9.375 0 1 0 0 18.75c.77 0 1.327-.475 1.52-1.093.189-.598.03-1.298-.484-1.797l-1.03-.998c-.806-.782-.252-2.148.87-2.148h5.63c1.316 0 2.657-.857 2.815-2.329A9.375 9.375 0 0 0 10 .625ZM11.875 5a1.25 1.25 0 1 1-2.5 0 1.25 1.25 0 0 1 2.5 0ZM6.25 8.125a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5ZM5 12.5A1.25 1.25 0 1 0 5 10a1.25 1.25 0 0 0 0 2.5Zm9.375-4.375a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M2.25 12A9.75 9.75 0 0 1 12 2.25a9.75 9.75 0 0 1 9.694 10.802c-.087.816-.868 1.456-1.886 1.456h-6.756c-2.695 0-4.023 3.277-2.088 5.153l1.235 1.198c.217.21.248.461.195.63a.361.361 0 0 1-.125.185.43.43 0 0 1-.269.076A9.75 9.75 0 0 1 2.25 12ZM12 .75C5.787.75.75 5.787.75 12S5.787 23.25 12 23.25c.924 0 1.592-.57 1.825-1.311.226-.719.036-1.558-.582-2.157l-1.235-1.198c-.967-.938-.303-2.576 1.044-2.576h6.756c1.578 0 3.188-1.03 3.377-2.796.043-.398.065-.803.065-1.212C23.25 5.787 18.213.75 12 .75ZM14.25 6a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0ZM7.5 9.75a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3ZM6 15a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Zm11.25-5.25a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z"; 

    protected override string IconGeometry32 { get; }
        = "M3 16C3 8.82 8.82 3 16 3s13 5.82 13 13c0 .474-.025.942-.075 1.403-.116 1.087-1.157 1.94-2.515 1.94h-9.007c-3.593 0-5.364 4.37-2.785 6.872l1.647 1.597c.29.28.331.615.26.84a.483.483 0 0 1-.166.247A.575.575 0 0 1 16 29C8.82 29 3 23.18 3 16ZM16 1C7.716 1 1 7.716 1 16c0 8.284 6.716 15 15 15 1.232 0 2.123-.76 2.433-1.749.301-.957.048-2.076-.775-2.875L16.01 24.78c-1.29-1.25-.404-3.436 1.393-3.436h9.007c2.105 0 4.251-1.371 4.504-3.727.057-.531.086-1.07.086-1.616 0-8.284-6.716-15-15-15Zm3 7a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm-9 5a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm-2 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm15-7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}