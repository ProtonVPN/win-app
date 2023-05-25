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

public class LockCheckFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M5.2 5H5a3 3 0 0 1 6 0H5.2ZM4 5.02V5a4 4 0 1 1 8 0v.02c.392.023.67.077.908.198a2 2 0 0 1 .874.874C14 6.52 14 7.08 14 8.2v3.6c0 1.12 0 1.68-.218 2.108a2 2 0 0 1-.874.874C12.48 15 11.92 15 10.8 15H5.2c-1.12 0-1.68 0-2.108-.218a2 2 0 0 1-.874-.874C2 13.48 2 12.92 2 11.8V8.2c0-1.12 0-1.68.218-2.108a2 2 0 0 1 .874-.874c.238-.121.516-.175.908-.199Zm7.854 2.626a.5.5 0 0 1 0 .708l-3.859 3.858a.7.7 0 0 1-.99 0l-1.859-1.858a.5.5 0 0 1 .708-.708L7.5 11.293l3.646-3.647a.5.5 0 0 1 .708 0Z";
}