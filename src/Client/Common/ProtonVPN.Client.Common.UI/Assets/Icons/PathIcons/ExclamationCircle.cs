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

public class ExclamationCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12Zm0 1A7 7 0 1 0 8 1a7 7 0 0 0 0 14ZM8 4a.941.941 0 0 1 .934 1.058l-.438 3.504a.5.5 0 0 1-.992 0l-.438-3.504A.941.941 0 0 1 8 4Zm0 7.4A.7.7 0 1 0 8 10a.7.7 0 0 0 0 1.4Z";

    protected override string IconGeometry20 { get; }
        = "M10 17.5a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm0 1.25a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5ZM10 5c.708 0 1.255.62 1.168 1.323l-.548 4.38a.625.625 0 0 1-1.24 0l-.548-4.38A1.177 1.177 0 0 1 10 5Zm0 9.25a.875.875 0 1 0 0-1.75.875.875 0 0 0 0 1.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0 1.5c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5ZM12 6c.85 0 1.507.744 1.401 1.587l-.657 5.256a.75.75 0 0 1-1.488 0l-.657-5.256A1.412 1.412 0 0 1 12 6Zm0 11.1a1.05 1.05 0 1 0 0-2.1 1.05 1.05 0 0 0 0 2.1Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 28c6.627 0 12-5.373 12-12S22.627 4 16 4 4 9.373 4 16s5.373 12 12 12Zm0 2c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Zm0-22c1.133 0 2.009.993 1.868 2.116l-.876 7.008a1 1 0 0 1-1.984 0l-.876-7.008A1.883 1.883 0 0 1 16 8Zm0 14.8a1.4 1.4 0 1 0 0-2.8 1.4 1.4 0 0 0 0 2.8Z";
}