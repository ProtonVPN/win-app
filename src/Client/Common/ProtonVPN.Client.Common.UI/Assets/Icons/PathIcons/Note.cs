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

public class Note : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 12a1 1 0 0 0 1 1h5v-2.5A1.5 1.5 0 0 1 10.5 9H13V4a1 1 0 0 0-1-1H4a1 1 0 0 0-1 1v8Zm9.75-2H10.5a.5.5 0 0 0-.5.5v2.25a1.5 1.5 0 0 0 .232-.19l2.329-2.328a1.5 1.5 0 0 0 .19-.232ZM4 14a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v5.172a2.5 2.5 0 0 1-.732 1.767l-2.329 2.329A2.5 2.5 0 0 1 9.172 14H4Z";
}