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

public class Emoji : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14.929 8.5a6.429 6.429 0 1 1-12.858 0 6.429 6.429 0 0 1 12.858 0ZM16 8.5a7.5 7.5 0 1 1-15 0 7.5 7.5 0 0 1 15 0Zm-4.821 0a1.071 1.071 0 1 0 0-2.143 1.071 1.071 0 0 0 0 2.143ZM6.893 7.429a1.071 1.071 0 1 1-2.143 0 1.071 1.071 0 0 1 2.143 0Zm-1.97 3.303c-.115-.316.14-.625.476-.625h6.202c.336 0 .591.309.476.626-.548 1.513-1.943 2.588-3.577 2.588s-3.029-1.075-3.577-2.588Z";

    protected override string IconGeometry20 { get; }
        = "M18.66 10.625a8.036 8.036 0 1 1-16.07 0 8.036 8.036 0 0 1 16.07 0Zm1.34 0a9.375 9.375 0 1 1-18.75 0 9.375 9.375 0 1 1 18.75 0Zm-6.027 0a1.34 1.34 0 1 0 0-2.679 1.34 1.34 0 0 0 0 2.679Zm-5.357-1.34a1.34 1.34 0 1 1-2.678 0 1.34 1.34 0 0 1 2.678 0Zm-2.463 4.13c-.143-.395.175-.781.596-.781h7.752c.42 0 .74.386.596.782-.686 1.891-2.43 3.236-4.472 3.236-2.043 0-3.786-1.345-4.472-3.236Z"; 

    protected override string IconGeometry24 { get; }
        = "M22.393 12.75a9.643 9.643 0 1 1-19.286 0 9.643 9.643 0 0 1 19.286 0Zm1.607 0C24 18.963 18.963 24 12.75 24S1.5 18.963 1.5 12.75 6.537 1.5 12.75 1.5 24 6.537 24 12.75Zm-7.232 0a1.607 1.607 0 1 0 0-3.214 1.607 1.607 0 0 0 0 3.214Zm-6.429-1.607a1.607 1.607 0 1 1-3.214 0 1.607 1.607 0 0 1 3.214 0Zm-2.955 4.956c-.172-.475.21-.938.715-.938H17.4c.505 0 .887.463.715.938-.823 2.27-2.915 3.883-5.366 3.883-2.451 0-4.543-1.613-5.366-3.883Z"; 

    protected override string IconGeometry32 { get; }
        = "M29.857 17c0 7.1-5.756 12.857-12.857 12.857-7.1 0-12.857-5.756-12.857-12.857C4.143 9.9 9.899 4.143 17 4.143c7.1 0 12.857 5.756 12.857 12.857ZM32 17c0 8.284-6.716 15-15 15-8.284 0-15-6.716-15-15C2 8.716 8.716 2 17 2c8.284 0 15 6.716 15 15Zm-9.643 0a2.143 2.143 0 1 0 0-4.286 2.143 2.143 0 0 0 0 4.286Zm-8.571-2.143a2.143 2.143 0 1 1-4.286 0 2.143 2.143 0 0 1 4.286 0Zm-3.941 6.608c-.23-.633.28-1.25.953-1.25h12.404c.673 0 1.182.617.953 1.25-1.097 3.027-3.887 5.178-7.155 5.178-3.268 0-6.058-2.151-7.155-5.178Z";
}