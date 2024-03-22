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

public class UserArrowRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm-2.497 2a1.5 1.5 0 0 0-1.2.6l-1.2 1.6a.5.5 0 0 0 .4.8H9v1H3.503c-1.236 0-1.942-1.411-1.2-2.4l1.2-1.6a2.5 2.5 0 0 1 2-1H9v1H5.503Zm6.694 3.85a.5.5 0 0 0 .708.006L14.79 12a.7.7 0 0 0 0-.998l-1.886-1.857a.5.5 0 0 0-.702.712L13.364 11H9.508a.5.5 0 0 0 0 1h3.856l-1.161 1.144a.5.5 0 0 0-.006.707Z";

    protected override string IconGeometry20 { get; }
        = "M10 8.75a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5ZM10 10a3.75 3.75 0 1 0 0-7.5 3.75 3.75 0 0 0 0 7.5Zm-3.122 2.5c-.59 0-1.145.278-1.5.75l-1.5 2a.625.625 0 0 0 .5 1h6.872v1.25H4.378c-1.545 0-2.427-1.764-1.5-3l1.5-2a3.125 3.125 0 0 1 2.5-1.25h4.372v1.25H6.878Zm8.369 4.814a.625.625 0 0 0 .884.006l2.357-2.322a.875.875 0 0 0 0-1.246l-2.357-2.322a.625.625 0 1 0-.877.89l1.451 1.43h-4.82a.625.625 0 1 0 0 1.25h4.82l-1.451 1.43a.625.625 0 0 0-.007.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 10.5a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm0 1.5a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9Zm-3.746 3a2.25 2.25 0 0 0-1.8.9l-1.8 2.4a.75.75 0 0 0 .6 1.2H13.5V21H5.254c-1.854 0-2.912-2.117-1.8-3.6l1.8-2.4a3.75 3.75 0 0 1 3-1.5H13.5V15H8.254Zm10.042 5.776a.75.75 0 0 0 1.06.008l2.83-2.786a1.05 1.05 0 0 0 0-1.496l-2.83-2.786a.75.75 0 0 0-1.052 1.068l1.742 1.716h-5.784a.75.75 0 0 0 0 1.5h5.784l-1.742 1.716a.75.75 0 0 0-.008 1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 14a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm0 2a6 6 0 1 0 0-12 6 6 0 0 0 0 12Zm-4.995 4a3 3 0 0 0-2.4 1.2l-2.4 3.2a1 1 0 0 0 .8 1.6H18v2H7.005c-2.472 0-3.883-2.822-2.4-4.8l2.4-3.2a5 5 0 0 1 4-2H18v2h-6.995Zm13.39 7.702a1 1 0 0 0 1.414.01l3.772-3.715a1.4 1.4 0 0 0 0-1.994l-3.772-3.715a1 1 0 0 0-1.403 1.424L26.728 22h-7.713a1 1 0 1 0 0 2h7.713l-2.322 2.288a1 1 0 0 0-.011 1.414Z";
}