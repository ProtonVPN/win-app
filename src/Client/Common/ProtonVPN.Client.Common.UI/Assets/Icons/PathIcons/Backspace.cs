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

public class Backspace : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.518 2a2.5 2.5 0 0 0-2.021 1.03L.096 7.706a.5.5 0 0 0 0 .588l3.4 4.676A2.5 2.5 0 0 0 5.519 14H15.5a.5.5 0 0 0 .5-.5v-11a.5.5 0 0 0-.5-.5H5.518ZM4.305 3.618A1.5 1.5 0 0 1 5.518 3H15v10H5.518a1.5 1.5 0 0 1-1.213-.618L1.118 8l3.187-4.382Zm3.549 2.028a.5.5 0 1 0-.708.708L8.793 8 7.146 9.646a.5.5 0 0 0 .708.708L9.5 8.707l1.646 1.647a.5.5 0 0 0 .708-.708L10.207 8l1.647-1.646a.5.5 0 0 0-.708-.708L9.5 7.293 7.854 5.646Z";

    protected override string IconGeometry20 { get; }
        = "M6.898 2.5c-1 0-1.94.478-2.527 1.287L.12 9.632a.625.625 0 0 0 0 .736l4.25 5.845A3.125 3.125 0 0 0 6.899 17.5h12.477c.345 0 .625-.28.625-.625V3.125a.625.625 0 0 0-.625-.625H6.898ZM5.382 4.522a1.875 1.875 0 0 1 1.516-.772H18.75v12.5H6.898c-.6 0-1.163-.287-1.516-.772L1.398 10l3.984-5.478Zm4.435 2.536a.625.625 0 1 0-.884.884L10.991 10l-2.058 2.058a.625.625 0 1 0 .884.884l2.058-2.058 2.058 2.058a.625.625 0 1 0 .884-.884L12.759 10l2.058-2.058a.625.625 0 1 0-.884-.884l-2.058 2.058-2.058-2.058Z"; 

    protected override string IconGeometry24 { get; }
        = "M8.278 3a3.75 3.75 0 0 0-3.033 1.544L.143 11.56a.75.75 0 0 0 0 .882l5.102 7.015A3.75 3.75 0 0 0 8.278 21H23.25a.75.75 0 0 0 .75-.75V3.75a.75.75 0 0 0-.75-.75H8.278Zm-1.82 2.427a2.25 2.25 0 0 1 1.82-.927H22.5v15H8.278a2.25 2.25 0 0 1-1.82-.927L1.678 12l4.78-6.573ZM11.78 8.47a.75.75 0 1 0-1.06 1.06L13.19 12l-2.47 2.47a.75.75 0 1 0 1.06 1.06l2.47-2.47 2.47 2.47a.75.75 0 1 0 1.06-1.06L15.31 12l2.47-2.47a.75.75 0 0 0-1.06-1.06l-2.47 2.47-2.47-2.47Z"; 

    protected override string IconGeometry32 { get; }
        = "M11.037 4a5 5 0 0 0-4.044 2.06L.191 15.411a1 1 0 0 0 0 1.176l6.802 9.353A5 5 0 0 0 11.037 28H31a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1H11.037ZM8.61 7.235A3 3 0 0 1 11.037 6H30v20H11.037a3 3 0 0 1-2.426-1.235L2.236 16l6.375-8.765Zm7.096 4.058a1 1 0 0 0-1.414 1.414L17.586 16l-3.293 3.293a1 1 0 0 0 1.414 1.414L19 17.414l3.293 3.293a1 1 0 0 0 1.414-1.414L20.414 16l3.293-3.293a1 1 0 0 0-1.414-1.414L19 14.586l-3.293-3.293Z";
}