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

public class Folder : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M14 12V5.5a1 1 0 0 0-1-1H8.567a1.2 1.2 0 0 1-.72-.24L6.433 3.2a1 1 0 0 0-.6-.2H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1ZM3 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5.5a2 2 0 0 0-2-2H8.567a.2.2 0 0 1-.12-.04L7.033 2.4a2 2 0 0 0-1.2-.4H3Z";
}