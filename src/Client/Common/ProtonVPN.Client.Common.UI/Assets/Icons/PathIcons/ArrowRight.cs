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

public class ArrowRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.684 2.647a.5.5 0 0 0 .002.708L12.364 8H2.5a.5.5 0 1 0 0 1h9.864l-4.678 4.645a.5.5 0 1 0 .705.71l5.396-5.36a.695.695 0 0 0 .198-.39.5.5 0 0 0 0-.211.695.695 0 0 0-.198-.39L8.39 2.645a.5.5 0 0 0-.707.002Z";

    protected override string IconGeometry20 { get; }
        = "M9.604 3.31a.625.625 0 0 0 .003.883L15.454 10H3.125a.625.625 0 1 0 0 1.25h12.33l-5.848 5.806a.625.625 0 1 0 .881.887l6.746-6.699a.87.87 0 0 0 .248-.487.628.628 0 0 0 0-.265.87.87 0 0 0-.248-.487l-6.746-6.699a.625.625 0 0 0-.884.003Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.525 3.971a.75.75 0 0 0 .004 1.06L18.545 12H3.75a.75.75 0 0 0 0 1.5h14.795l-7.016 6.968a.75.75 0 0 0 1.057 1.064l8.094-8.039c.166-.164.265-.37.298-.584a.75.75 0 0 0 0-.318 1.044 1.044 0 0 0-.298-.585l-8.094-8.039a.75.75 0 0 0-1.06.004Z"; 

    protected override string IconGeometry32 { get; }
        = "M15.367 5.295a1 1 0 0 0 .005 1.414L24.727 16H5a1 1 0 1 0 0 2h19.727l-9.355 9.291a1 1 0 1 0 1.41 1.42L27.573 17.99c.22-.22.353-.494.397-.78a1.006 1.006 0 0 0 0-.423 1.391 1.391 0 0 0-.397-.78L16.78 5.29a1 1 0 0 0-1.414.005Z";
}