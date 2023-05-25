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

public class LocksFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M6 4c-.582.005-.916.028-1.181.163a1.5 1.5 0 0 0-.656.656C4 5.139 4 5.559 4 6.4v4.2c0 .84 0 1.26.163 1.581a1.5 1.5 0 0 0 .656.655c.32.164.74.164 1.581.164h5.2c.84 0 1.26 0 1.581-.164a1.5 1.5 0 0 0 .655-.655c.164-.32.164-.74.164-1.581V6.4c0-.84 0-1.26-.164-1.581a1.5 1.5 0 0 0-.655-.656c-.265-.135-.598-.158-1.181-.162a3 3 0 1 0-6 0Zm.857 0h4.286a2.143 2.143 0 0 0-4.286 0Zm2.614 4.383a1 1 0 1 0-.942 0l-.436 1.744a.3.3 0 0 0 .291.373h1.232a.3.3 0 0 0 .29-.373l-.435-1.744Z M3 6.5a2.5 2.5 0 0 0-.728.865C2 7.9 2 8.6 2 10v1c0 1.4 0 2.1.272 2.635a2.5 2.5 0 0 0 1.093 1.092C3.9 15 4.6 15 6 15h2c1.4 0 2.1 0 2.635-.273.342-.174.637-.423.865-.727H6.2c-1.12 0-1.68 0-2.108-.218a2 2 0 0 1-.874-.874C3 12.48 3 11.92 3 10.8V6.5Z";
}