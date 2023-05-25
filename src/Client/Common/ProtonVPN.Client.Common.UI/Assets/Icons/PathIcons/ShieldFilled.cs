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

public class ShieldFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 2.029 3.07 3.723c-.16 1.784-.077 3.66.587 5.403.67 1.761 1.958 3.456 4.343 4.802 2.385-1.346 3.673-3.04 4.343-4.802.664-1.743.748-3.62.587-5.403L8 2.03Zm-.16-1.002-5.4 1.856a.495.495 0 0 0-.332.415c-.418 3.917.17 8.692 5.664 11.645a.488.488 0 0 0 .459 0c5.492-2.953 6.08-7.728 5.663-11.645a.495.495 0 0 0-.333-.415L8.162 1.027a.497.497 0 0 0-.323 0ZM4.222 4.203l3.67-1.325a.31.31 0 0 1 .216 0l3.67 1.325A.35.35 0 0 1 12 4.5c.278 2.798-.185 6.209-3.847 8.317a.307.307 0 0 1-.306 0C4.185 10.71 3.721 7.298 4 4.5a.35.35 0 0 1 .222-.297Z";
}