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

public class Rocket : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7 9a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0Zm1.5-.5a.5.5 0 1 0 0 1 .5.5 0 0 0 0-1Z M9.07 1.29a.957.957 0 0 0-1.14 0C5.12 3.378 5.004 6.776 5 9.55A2.5 2.5 0 0 0 3 12v2h11v-2a2.5 2.5 0 0 0-2-2.45c-.005-2.775-.121-6.173-2.93-8.26ZM4 12a1.5 1.5 0 0 1 1-1.415V13H4v-1Zm2 1v-3c0-1.35.007-2.726.25-4h4.5c.243 1.274.25 2.65.25 4v3H6Zm4.5-8h-4c.356-1.112.963-2.108 2-2.888 1.037.78 1.644 1.776 2 2.888Zm2.5 8h-1v-2.415A1.5 1.5 0 0 1 13 12v1Z M5.5 15a.5.5 0 0 0 0 1h6a.5.5 0 0 0 0-1h-6Z";
}