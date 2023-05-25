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
    protected override string IconGeometry { get; }
        = "M8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12Zm0 1A7 7 0 1 0 8 1a7 7 0 0 0 0 14ZM8 4a.941.941 0 0 1 .934 1.058l-.438 3.504a.5.5 0 0 1-.992 0l-.438-3.504A.941.941 0 0 1 8 4Zm0 7.4A.7.7 0 1 0 8 10a.7.7 0 0 0 0 1.4Z";
}