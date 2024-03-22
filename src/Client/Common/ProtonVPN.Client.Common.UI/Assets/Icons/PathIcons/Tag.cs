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

public class Tag : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m9.425 14.375 4.95-4.95L7.95 3.002H3v4.95l6.425 6.424Zm-.707.707a1 1 0 0 0 1.414 0l4.95-4.95a1 1 0 0 0 0-1.414L8.657 2.294A1 1 0 0 0 7.95 2H3a1 1 0 0 0-1 1v4.95a1 1 0 0 0 .293.707l6.425 6.425ZM5.5 6.5a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z";

    protected override string IconGeometry20 { get; }
        = "m11.781 17.969 6.187-6.187-8.03-8.031H3.75v6.187l8.031 8.03Zm-.884.884a1.25 1.25 0 0 0 1.768 0l6.187-6.188a1.25 1.25 0 0 0 0-1.767l-8.03-8.031a1.25 1.25 0 0 0-.885-.366H3.75c-.69 0-1.25.56-1.25 1.25v6.187c0 .332.132.65.366.884l8.031 8.03ZM6.875 8.125a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "m14.137 21.563 7.425-7.425L11.925 4.5H4.5v7.425l9.637 9.636Zm-1.06 1.06a1.5 1.5 0 0 0 2.12 0l7.425-7.425a1.5 1.5 0 0 0 0-2.12L12.986 3.44a1.5 1.5 0 0 0-1.061-.439H4.5A1.5 1.5 0 0 0 3 4.5v7.425c0 .397.158.779.44 1.06l9.636 9.637ZM8.25 9.75a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z"; 

    protected override string IconGeometry32 { get; }
        = "m18.85 28.75 9.9-9.9L15.9 6.002H6v9.9L18.85 28.75Zm-1.415 1.414a2 2 0 0 0 2.829 0l9.9-9.9a2 2 0 0 0 0-2.828L17.313 4.587a2 2 0 0 0-1.414-.586H6a2 2 0 0 0-2 2v9.9a2 2 0 0 0 .586 1.414l12.85 12.85ZM11 13a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}