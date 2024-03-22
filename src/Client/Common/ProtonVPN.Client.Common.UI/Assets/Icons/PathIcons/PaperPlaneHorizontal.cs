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

public class PaperPlaneHorizontal : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m12.444 8.007-10.23-5.11L3.396 8l9.047.006ZM3.391 9l9.011.006L2.22 14.104l1.17-5.103ZM1.014 2.154a.532.532 0 0 1 .756-.596l12.936 6.46a.534.534 0 0 1 0 .954L1.78 15.442a.533.533 0 0 1-.756-.597l1.405-6.118a1.068 1.068 0 0 0 0-.48L1.014 2.154Z";

    protected override string IconGeometry20 { get; }
        = "M15.556 9.383 2.766 2.996l1.48 6.38 11.31.007ZM4.239 10.626l11.264.007-12.73 6.372 1.466-6.379ZM1.268 2.067a.664.664 0 0 1 .945-.745l16.17 8.076c.49.246.49.947 0 1.192l-16.16 8.088a.666.666 0 0 1-.945-.746l1.757-7.648a1.335 1.335 0 0 0 0-.6L1.267 2.067Z"; 

    protected override string IconGeometry24 { get; }
        = "M18.667 11.26 3.32 3.595l1.775 7.656 13.572.01Zm-13.58 1.491 13.517.009-15.276 7.646 1.758-7.655ZM1.521 2.481a.797.797 0 0 1 1.133-.894l19.403 9.69a.8.8 0 0 1 .001 1.43L2.67 22.415a.8.8 0 0 1-1.135-.896l2.108-9.178a1.602 1.602 0 0 0 0-.719l-2.12-9.14Z"; 

    protected override string IconGeometry32 { get; }
        = "M24.889 15.013 4.426 4.793l2.368 10.209 18.095.011ZM6.782 17.002l18.023.011L4.438 27.208l2.344-10.206ZM2.029 3.308c-.203-.88.706-1.596 1.511-1.193l25.871 12.922c.785.393.785 1.514.002 1.906L3.558 29.886c-.804.403-1.715-.316-1.513-1.194l2.81-12.237a2.136 2.136 0 0 0 0-.96L2.03 3.309Z";
}