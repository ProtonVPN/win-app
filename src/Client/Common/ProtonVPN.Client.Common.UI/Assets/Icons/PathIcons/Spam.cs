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

public class Spam : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.942 4a.941.941 0 0 1 .934 1.058l-.438 3.504a.5.5 0 0 1-.993 0l-.438-3.504A.941.941 0 0 1 7.942 4Zm-.001 7.4a.7.7 0 1 0 0-1.4.7.7 0 0 0 0 1.4Z M5.414 14h5.172L14 10.586V5.414L10.586 2H5.414L2 5.414v5.172L5.414 14Zm0-13a1 1 0 0 0-.707.293L1.293 4.707A1 1 0 0 0 1 5.414v5.172a1 1 0 0 0 .293.707l3.414 3.414a1 1 0 0 0 .707.293h5.172a1 1 0 0 0 .707-.293l3.414-3.414a1 1 0 0 0 .293-.707V5.414a1 1 0 0 0-.293-.707l-3.414-3.414A1 1 0 0 0 10.586 1H5.414Z";

    protected override string IconGeometry20 { get; }
        = ""; 

    protected override string IconGeometry24 { get; }
        = ""; 

    protected override string IconGeometry32 { get; }
        = "";
}