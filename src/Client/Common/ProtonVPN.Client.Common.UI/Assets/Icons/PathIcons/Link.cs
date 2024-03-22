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
    protected override string IconGeometry16 { get; }
        = "M9.023 2.027a3.5 3.5 0 0 1 4.95 4.95l-2.437 2.437a.5.5 0 1 1-.707-.707l2.437-2.437A2.5 2.5 0 1 0 9.73 2.734L7.293 5.171a.5.5 0 1 1-.707-.707l2.437-2.437Zm1.098 3.852a.5.5 0 0 1 0 .707l-3.535 3.535a.5.5 0 1 1-.708-.707L9.414 5.88a.5.5 0 0 1 .707 0ZM2.03 13.97a3.5 3.5 0 0 0 4.95 0l2.44-2.439a.5.5 0 1 0-.708-.707l-2.44 2.44a2.5 2.5 0 1 1-3.535-3.536l2.44-2.44a.5.5 0 0 0-.707-.707l-2.44 2.44a3.5 3.5 0 0 0 0 4.95Z";

    protected override string IconGeometry20 { get; }
        = "M11.279 2.534a4.375 4.375 0 0 1 6.187 6.187l-3.047 3.047a.625.625 0 1 1-.883-.884l3.046-3.047a3.125 3.125 0 1 0-4.42-4.42L9.117 6.465a.625.625 0 0 1-.884-.884l3.047-3.046Zm1.372 4.815a.625.625 0 0 1 0 .884l-4.419 4.419a.625.625 0 1 1-.884-.884l4.42-4.42a.625.625 0 0 1 .883 0ZM2.536 17.463a4.375 4.375 0 0 0 6.187 0l3.05-3.05a.625.625 0 1 0-.884-.883l-3.05 3.05a3.125 3.125 0 1 1-4.419-4.42l3.05-3.05a.625.625 0 0 0-.884-.883l-3.05 3.049a4.375 4.375 0 0 0 0 6.187Z"; 

    protected override string IconGeometry24 { get; }
        = "M13.535 3.04a5.25 5.25 0 1 1 7.424 7.425l-3.656 3.656a.75.75 0 1 1-1.06-1.06l3.656-3.656A3.75 3.75 0 1 0 14.595 4.1L10.94 7.757a.75.75 0 0 1-1.061-1.06l3.656-3.656Zm1.647 5.778a.75.75 0 0 1 0 1.061l-5.304 5.303a.75.75 0 0 1-1.06-1.06l5.303-5.304a.75.75 0 0 1 1.06 0ZM3.044 20.956a5.25 5.25 0 0 0 7.424 0l3.66-3.66a.75.75 0 1 0-1.061-1.06l-3.66 3.66a3.75 3.75 0 0 1-5.303-5.304l3.66-3.66a.75.75 0 0 0-1.061-1.06l-3.66 3.66a5.25 5.25 0 0 0 0 7.424Z"; 

    protected override string IconGeometry32 { get; }
        = "M18.046 4.054a7 7 0 1 1 9.9 9.9l-4.875 4.874a1 1 0 1 1-1.414-1.414l4.875-4.875a5 5 0 1 0-7.072-7.07l-4.874 4.874a1 1 0 0 1-1.414-1.414l4.874-4.875Zm2.196 7.704a1 1 0 0 1 0 1.414l-7.07 7.071a1 1 0 0 1-1.415-1.414l7.071-7.071a1 1 0 0 1 1.414 0ZM4.058 27.94a7 7 0 0 0 9.9 0l4.879-4.879a1 1 0 0 0-1.415-1.414l-4.879 4.879a5 5 0 1 1-7.07-7.071l4.878-4.88a1 1 0 0 0-1.414-1.413l-4.879 4.879a7 7 0 0 0 0 9.9Z";
}