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

public class Shield : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3.07 3.723c-.16 1.784-.077 3.66.587 5.403.67 1.761 1.958 3.456 4.343 4.802 2.385-1.346 3.673-3.04 4.343-4.802.664-1.743.748-3.62.587-5.403L8 2.03 3.07 3.723Zm-.63-.84 5.399-1.856a.497.497 0 0 1 .323 0l5.398 1.856a.495.495 0 0 1 .333.415c.417 3.917-.17 8.692-5.663 11.645a.488.488 0 0 1-.459 0C2.278 11.99 1.689 7.215 2.107 3.298a.495.495 0 0 1 .333-.415Z";
}