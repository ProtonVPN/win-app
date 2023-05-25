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

public class ClockRotateLeft : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3.074 7.596A5.501 5.501 0 0 1 14 8.5a5.5 5.5 0 0 1-9.9 3.3l-.8.6a6.5 6.5 0 1 0-1.205-5.013l-.741-.74a.5.5 0 1 0-.708.707l1.5 1.5a.5.5 0 0 0 .602.08l1.75-1a.5.5 0 1 0-.496-.868l-.928.53Z M9 5.5a.5.5 0 0 0-1 0v3.207l1.646 1.647a.5.5 0 0 0 .708-.708L9 8.293V5.5Z";
}