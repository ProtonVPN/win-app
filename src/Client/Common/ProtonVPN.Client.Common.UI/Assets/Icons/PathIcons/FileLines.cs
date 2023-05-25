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

public class FileLines : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M13 6h-2.5A1.5 1.5 0 0 1 9 4.5V2H4a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V6Zm-3-3.586L12.586 5H10.5a.5.5 0 0 1-.5-.5V2.414ZM4 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V5.828a2 2 0 0 0-.586-1.414l-2.828-2.828A2 2 0 0 0 9.172 1H4Zm1 6a.5.5 0 0 0 0 1h5.5a.5.5 0 0 0 0-1H5Zm-.5 2.5A.5.5 0 0 1 5 9h3.5a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5ZM5 11a.5.5 0 0 0 0 1h5.5a.5.5 0 0 0 0-1H5Z";
}