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

public class LockOpenFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 0a4 4 0 0 0-4 4v1.02c-.392.023-.67.077-.908.198a2 2 0 0 0-.874.874C2 6.52 2 7.08 2 8.2v3.6c0 1.12 0 1.68.218 2.108a2 2 0 0 0 .874.874C3.52 15 4.08 15 5.2 15h5.6c1.12 0 1.68 0 2.108-.218a2 2 0 0 0 .874-.874C14 13.48 14 12.92 14 11.8V8.2c0-1.12 0-1.68-.218-2.108a2 2 0 0 0-.874-.874C12.48 5 11.92 5 10.8 5H5V4a3 3 0 0 1 5.572-1.546.5.5 0 0 0 .856-.516A3.998 3.998 0 0 0 8 0Zm.47 9.883a1 1 0 1 0-.94 0l-.437 1.744a.3.3 0 0 0 .291.373h1.232a.3.3 0 0 0 .29-.373l-.435-1.744Z";
}