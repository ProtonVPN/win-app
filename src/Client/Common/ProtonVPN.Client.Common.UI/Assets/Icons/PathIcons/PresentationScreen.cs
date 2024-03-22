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

public class PresentationScreen : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 1.5a.5.5 0 0 1 .5-.5h14a.5.5 0 0 1 0 1h-14a.5.5 0 0 1-.5-.5Zm5.584 13.277a.5.5 0 0 1 .139-.693L8 13.232V11H3.5A1.5 1.5 0 0 1 2 9.5V3h1v6.5a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V3h1v6.5a1.5 1.5 0 0 1-1.5 1.5H9v2.232l1.277.852a.5.5 0 1 1-.554.832L8.5 14.101l-1.223.815a.5.5 0 0 1-.693-.139Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 1.875c0-.345.28-.625.625-.625h17.5a.625.625 0 1 1 0 1.25h-17.5a.625.625 0 0 1-.625-.625Zm6.98 16.597a.625.625 0 0 1 .173-.867L10 16.541V13.75H4.375A1.875 1.875 0 0 1 2.5 11.875V3.75h1.25v8.125c0 .345.28.625.625.625h12.5c.345 0 .625-.28.625-.625V3.75h1.25v8.125c0 1.036-.84 1.875-1.875 1.875H11.25v2.79l1.597 1.065a.625.625 0 1 1-.694 1.04l-1.528-1.019-1.528 1.019a.625.625 0 0 1-.867-.173Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 2.25a.75.75 0 0 1 .75-.75h21a.75.75 0 0 1 0 1.5h-21a.75.75 0 0 1-.75-.75Zm8.376 19.916a.75.75 0 0 1 .208-1.04L12 19.849V16.5H5.25A2.25 2.25 0 0 1 3 14.25V4.5h1.5v9.75c0 .414.336.75.75.75h15a.75.75 0 0 0 .75-.75V4.5h1.5v9.75a2.25 2.25 0 0 1-2.25 2.25H13.5v3.349l1.916 1.277a.75.75 0 1 1-.832 1.248l-1.834-1.223-1.834 1.223a.75.75 0 0 1-1.04-.208Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 3a1 1 0 0 1 1-1h28a1 1 0 1 1 0 2H3a1 1 0 0 1-1-1Zm11.168 26.555a1 1 0 0 1 .277-1.387L16 26.465V22H7a3 3 0 0 1-3-3V6h2v13a1 1 0 0 0 1 1h20a1 1 0 0 0 1-1V6h2v13a3 3 0 0 1-3 3h-9v4.465l2.555 1.703a1 1 0 0 1-1.11 1.664L17 28.202l-2.445 1.63a1 1 0 0 1-1.387-.277Z";
}