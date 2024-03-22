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

public class KeySkeleton : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 4.414a2.414 2.414 0 1 1-4.829 0 2.414 2.414 0 0 1 4.83 0Zm1 0A3.414 3.414 0 0 1 8.554 7.16l-3.832 3.89 1.374 1.373a.5.5 0 1 1-.707.707l-1.371-1.37L3 12.775l1.371 1.37a.5.5 0 1 1-.707.708l-1.371-1.371a1 1 0 0 1 0-1.414l1.37-1.37 4.183-4.246A3.414 3.414 0 1 1 14 4.414Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 5.518a3.018 3.018 0 1 1-6.036 0 3.018 3.018 0 0 1 6.036 0Zm1.25 0a4.268 4.268 0 0 1-6.807 3.43l-4.79 4.863 1.716 1.717a.625.625 0 1 1-.884.884l-1.713-1.714L3.75 15.97l1.714 1.713a.625.625 0 1 1-.884.884l-1.714-1.713a1.25 1.25 0 0 1 0-1.768l1.712-1.712 5.23-5.308A4.268 4.268 0 1 1 17.5 5.518Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 6.622a3.622 3.622 0 1 1-7.243 0 3.622 3.622 0 0 1 7.243 0Zm1.5 0a5.122 5.122 0 0 1-8.169 4.117l-5.748 5.834 2.06 2.06a.75.75 0 1 1-1.06 1.06l-2.057-2.056L4.5 19.165l2.056 2.056a.75.75 0 1 1-1.06 1.06l-2.057-2.056a1.5 1.5 0 0 1 0-2.121l2.055-2.055 6.275-6.368a5.122 5.122 0 1 1 9.23-3.058Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 8.829a4.829 4.829 0 1 1-9.658 0 4.829 4.829 0 0 1 9.658 0Zm2 0a6.829 6.829 0 0 1-10.892 5.49l-7.664 7.778 2.747 2.747a1 1 0 1 1-1.414 1.415l-2.742-2.742L6 25.552l2.742 2.741a1 1 0 1 1-1.415 1.415l-2.742-2.742a2 2 0 0 1 0-2.829l2.74-2.739 8.367-8.492A6.829 6.829 0 1 1 28 8.83Z";
}