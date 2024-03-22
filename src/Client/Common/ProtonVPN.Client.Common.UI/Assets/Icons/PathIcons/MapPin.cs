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

public class MapPin : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 2C5.783 2 4 3.78 4 5.96a5.88 5.88 0 0 0 .906 3.133l2.929 4.648A11.062 11.062 0 0 0 8 14l.012-.018c.034-.05.078-.12.153-.24l2.929-4.648A5.88 5.88 0 0 0 12 5.961C12 3.78 10.217 2 8 2ZM4.06 9.626A6.877 6.877 0 0 1 3 5.96C3 3.22 5.239 1 8 1s5 2.22 5 4.96a6.877 6.877 0 0 1-1.06 3.666l-2.929 4.648c-.143.228-.215.342-.282.413a1.006 1.006 0 0 1-1.458 0c-.067-.07-.139-.185-.282-.413L4.06 9.626Z M8 7a1 1 0 1 0 0-2 1 1 0 0 0 0 2Zm0 1a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";

    protected override string IconGeometry20 { get; }
        = "M10 2.5c-2.77 0-5 2.226-5 4.95 0 1.384.392 2.741 1.133 3.917l3.66 5.81a13.742 13.742 0 0 0 .207.321l.015-.021c.042-.063.098-.151.192-.3l3.66-5.81A7.346 7.346 0 0 0 15 7.45c0-2.724-2.23-4.95-5-4.95Zm-4.925 9.533A8.596 8.596 0 0 1 3.75 7.45c0-3.424 2.798-6.2 6.25-6.2s6.25 2.776 6.25 6.2c0 1.62-.46 3.208-1.325 4.583l-3.66 5.81c-.18.285-.27.427-.354.516a1.257 1.257 0 0 1-1.822 0c-.084-.089-.174-.231-.353-.516l-3.661-5.81Z M10 8.75a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5ZM10 10a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 3C8.675 3 6 5.67 6 8.94c0 1.66.47 3.29 1.36 4.7l4.392 6.972a16.527 16.527 0 0 0 .248.386l.018-.026c.05-.076.117-.181.23-.36l4.393-6.972A8.815 8.815 0 0 0 18 8.94C18 5.67 15.325 3 12 3ZM6.09 14.44a10.315 10.315 0 0 1-1.59-5.5C4.5 4.83 7.858 1.5 12 1.5c4.142 0 7.5 3.33 7.5 7.44 0 1.945-.55 3.85-1.59 5.5l-4.393 6.972c-.215.341-.323.512-.423.618a1.508 1.508 0 0 1-2.188 0c-.1-.106-.208-.277-.423-.619L6.09 14.44Z M12 10.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Zm0 1.5a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 4c-4.434 0-8 3.561-8 7.92 0 2.215.627 4.386 1.812 6.266l5.857 9.297a22.246 22.246 0 0 0 .331.514l.024-.034c.067-.101.157-.242.307-.48l5.857-9.297A11.754 11.754 0 0 0 24 11.92C24 7.56 20.433 4 16 4ZM8.12 19.253A13.753 13.753 0 0 1 6 11.92C6 6.44 10.477 2 16 2s10 4.441 10 9.92c0 2.593-.735 5.134-2.12 7.333l-5.857 9.296c-.287.455-.43.683-.565.825a2.01 2.01 0 0 1-2.916 0c-.134-.142-.278-.37-.565-.825L8.12 19.253Z M16 14a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 2a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z";
}