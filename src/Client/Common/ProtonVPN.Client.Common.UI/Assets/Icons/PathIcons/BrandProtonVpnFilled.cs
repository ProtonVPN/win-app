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

public class BrandProtonVpnFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1.209 3.882c-.639-1.142.288-2.526 1.587-2.37l10.796 1.294a1.6 1.6 0 0 1 1.159 2.449l-5.457 8.55a1.5 1.5 0 0 1-2.573-.074l-5.512-9.85Zm1.468-1.377a.6.6 0 0 0-.595.888l.273.489L10.97 5.04c.553.074.839.7.534 1.166l-4.23 6.466.32.57a.5.5 0 0 0 .857.026l5.457-8.551a.6.6 0 0 0-.434-.919L2.676 2.505Z";

    protected override string IconGeometry20 { get; }
        = "M1.512 4.852c-.799-1.427.36-3.157 1.983-2.962L16.99 3.507a2 2 0 0 1 1.448 3.062l-6.821 10.688c-.763 1.195-2.525 1.144-3.217-.093L1.511 4.852Zm1.834-1.721a.75.75 0 0 0-.744 1.11l.342.611L13.713 6.3a.948.948 0 0 1 .667 1.459L9.092 15.84l.4.714a.625.625 0 0 0 1.072.03l6.821-10.688a.75.75 0 0 0-.543-1.148L3.346 3.131Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.814 5.823c-.958-1.713.431-3.789 2.38-3.555l16.195 1.94c1.756.21 2.689 2.184 1.738 3.674L13.94 20.71c-.915 1.433-3.029 1.372-3.86-.112L1.814 5.823Zm2.201-2.066a.9.9 0 0 0-.892 1.333l.41.733 12.922 1.736c.83.112 1.259 1.05.8 1.75l-6.344 9.698.48.857a.75.75 0 0 0 1.286.037l8.185-12.826a.9.9 0 0 0-.651-1.377L4.014 3.757Z"; 

    protected override string IconGeometry32 { get; }
        = "M2.418 7.763c-1.277-2.283.576-5.05 3.174-4.74l21.593 2.588c2.342.28 3.586 2.911 2.317 4.899L18.59 27.61c-1.22 1.912-4.04 1.83-5.147-.149L2.418 7.763ZM5.354 5.01a1.2 1.2 0 0 0-1.19 1.778l.547.977 17.23 2.315a1.517 1.517 0 0 1 1.067 2.334l-8.46 12.93.639 1.143a1 1 0 0 0 1.716.05L27.816 9.433a1.2 1.2 0 0 0-.869-1.837L5.354 5.009Z";
}