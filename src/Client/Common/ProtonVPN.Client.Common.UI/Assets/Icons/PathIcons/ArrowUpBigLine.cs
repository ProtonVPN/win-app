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

public class ArrowUpBigLine : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7 6.545v-1H5.317L8.5 2.17l3.183 3.376H10v6.478H7V6.545Zm4 0v5.853c0 .345-.28.625-.625.625h-3.75A.625.625 0 0 1 6 12.398V6.545H4.448a.625.625 0 0 1-.455-1.054l4.052-4.297a.625.625 0 0 1 .91 0l4.052 4.297a.625.625 0 0 1-.455 1.054H11Zm-7.5 7.456a.5.5 0 0 0 0 1h10a.5.5 0 1 0 0-1h-10Z";

    protected override string IconGeometry20 { get; }
        = "M8.75 8.181v-1.25H6.646l3.979-4.22 3.979 4.22H12.5v8.099H8.75V8.18Zm5 0v7.317a.78.78 0 0 1-.781.78H8.28a.781.781 0 0 1-.781-.78V8.18H5.56a.781.781 0 0 1-.568-1.317l5.064-5.372a.781.781 0 0 1 1.137 0l5.065 5.372a.781.781 0 0 1-.568 1.317h-1.94Zm-9.375 9.32a.625.625 0 1 0 0 1.25h12.5a.625.625 0 1 0 0-1.25h-12.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.5 9.818v-1.5H7.976l4.774-5.065 4.774 5.065H15v9.717h-4.5V9.818Zm6 0v8.78c0 .517-.42.937-.938.937H9.938A.937.937 0 0 1 9 18.597v-8.78H6.672c-.822 0-1.246-.982-.682-1.58l6.078-6.447a.938.938 0 0 1 1.364 0l6.078 6.447c.564.598.14 1.58-.682 1.58H16.5ZM5.25 21a.75.75 0 0 0 0 1.5h15a.75.75 0 0 0 0-1.5h-15Z"; 

    protected override string IconGeometry32 { get; }
        = "M14 13.09v-2h-3.366L17 4.338l6.366 6.752H20v12.956h-6V13.09Zm8 0v11.706c0 .69-.56 1.25-1.25 1.25h-7.5c-.69 0-1.25-.56-1.25-1.25V13.09H8.896c-1.096 0-1.661-1.31-.91-2.107l8.104-8.596a1.25 1.25 0 0 1 1.82 0l8.103 8.596c.752.797.187 2.107-.91 2.107H22ZM7 28.002a1 1 0 1 0 0 2h20a1 1 0 1 0 0-2H7Z";
}