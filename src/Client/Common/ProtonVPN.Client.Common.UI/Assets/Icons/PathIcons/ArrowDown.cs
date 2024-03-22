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

public class ArrowDown : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 2.002a.5.5 0 0 1 .5.5v9.857l4.144-4.21a.5.5 0 1 1 .712.702l-4.859 4.936-.001.002a.698.698 0 0 1-.39.198.5.5 0 0 1-.213 0 .698.698 0 0 1-.389-.198l-.001-.002-4.86-4.937a.5.5 0 1 1 .713-.701L8 12.359V2.502a.5.5 0 0 1 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 2.502c.345 0 .625.28.625.625v12.321l5.18-5.262a.625.625 0 0 1 .89.877l-6.073 6.17-.002.003a.871.871 0 0 1-.487.247.624.624 0 0 1-.266 0 .872.872 0 0 1-.487-.247l-.002-.002-6.073-6.17a.625.625 0 0 1 .89-.878L10 15.448V3.128c0-.346.28-.626.625-.626Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 3.003a.75.75 0 0 1 .75.75v14.785l6.215-6.314a.75.75 0 1 1 1.07 1.052l-7.29 7.404-.002.003c-.163.165-.37.264-.583.297a.754.754 0 0 1-.32 0 1.047 1.047 0 0 1-.584-.297l-.002-.003-7.289-7.404a.75.75 0 1 1 1.07-1.052L12 18.538V3.753a.75.75 0 0 1 .75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 4.003a1 1 0 0 1 1 1v19.714l8.287-8.419a1 1 0 1 1 1.426 1.403l-9.719 9.873-.003.003c-.218.22-.493.353-.778.397a1.008 1.008 0 0 1-.427 0 1.395 1.395 0 0 1-.777-.397l-.004-.003-9.718-9.873a1 1 0 0 1 1.426-1.403L16 24.718V5.002a1 1 0 0 1 1-1Z";
}