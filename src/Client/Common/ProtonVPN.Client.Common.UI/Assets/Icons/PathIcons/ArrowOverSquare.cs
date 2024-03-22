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

public class ArrowOverSquare : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m11.354 1.853-.647.647h.364c1.031 0 1.978.354 2.686.944C14.51 4.073 15 4.979 15 6a.5.5 0 1 1-1 0c0-.688-.327-1.324-.884-1.788-.523-.436-1.242-.712-2.044-.712h-.193l.475.475a.5.5 0 0 1-.708.707L9.374 3.409a.7.7 0 0 1 0-.99l1.272-1.273a.5.5 0 0 1 .708.707ZM2.5 5A1.5 1.5 0 0 0 1 6.5v7A1.5 1.5 0 0 0 2.5 15h7a1.5 1.5 0 0 0 1.5-1.5v-7A1.5 1.5 0 0 0 9.5 5h-7ZM2 6.5a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-.5.5h-7a.5.5 0 0 1-.5-.5v-7Z";

    protected override string IconGeometry20 { get; }
        = "m14.192 2.317-.808.808h.455c1.289 0 2.472.443 3.357 1.18.942.786 1.554 1.918 1.554 3.195a.625.625 0 1 1-1.25 0c0-.86-.41-1.655-1.105-2.235-.653-.545-1.552-.89-2.556-.89h-.24l.593.593a.625.625 0 1 1-.884.884l-1.59-1.59a.875.875 0 0 1 0-1.238l1.59-1.591a.625.625 0 1 1 .884.884ZM3.125 6.25c-1.036 0-1.875.84-1.875 1.875v8.75c0 1.035.84 1.875 1.875 1.875h8.75c1.036 0 1.875-.84 1.875-1.875v-8.75c0-1.036-.84-1.875-1.875-1.875h-8.75ZM2.5 8.125c0-.345.28-.625.625-.625h8.75c.345 0 .625.28.625.625v8.75c0 .345-.28.625-.625.625h-8.75a.625.625 0 0 1-.625-.625v-8.75Z"; 

    protected override string IconGeometry24 { get; }
        = "m17.03 2.78-.97.97h.547c1.547 0 2.967.531 4.028 1.416C21.766 6.109 22.5 7.468 22.5 9A.75.75 0 1 1 21 9c0-1.032-.491-1.986-1.326-2.682-.784-.654-1.862-1.068-3.067-1.068h-.289l.712.712a.75.75 0 0 1-1.06 1.06l-1.91-1.908a1.05 1.05 0 0 1 0-1.485l1.91-1.91a.75.75 0 1 1 1.06 1.061ZM3.75 7.5A2.25 2.25 0 0 0 1.5 9.75v10.5a2.25 2.25 0 0 0 2.25 2.25h10.5a2.25 2.25 0 0 0 2.25-2.25V9.75a2.25 2.25 0 0 0-2.25-2.25H3.75ZM3 9.75A.75.75 0 0 1 3.75 9h10.5a.75.75 0 0 1 .75.75v10.5a.75.75 0 0 1-.75.75H3.75a.75.75 0 0 1-.75-.75V9.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M22.707 3.707 21.414 5h.729c2.062 0 3.955.708 5.37 1.888C29.021 8.146 30 9.958 30 12a1 1 0 0 1-2 0c0-1.375-.655-2.648-1.768-3.576C25.186 7.552 23.75 7 22.142 7h-.384l.95.95a1 1 0 0 1-1.415 1.414l-2.546-2.546a1.4 1.4 0 0 1 0-1.98l2.546-2.545a1 1 0 1 1 1.414 1.414ZM5 10a3 3 0 0 0-3 3v14a3 3 0 0 0 3 3h14a3 3 0 0 0 3-3V13a3 3 0 0 0-3-3H5Zm-1 3a1 1 0 0 1 1-1h14a1 1 0 0 1 1 1v14a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V13Z";
}