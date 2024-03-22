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

public class House : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.367 1.373a1 1 0 0 1 1.266 0l5 4.09a1 1 0 0 1 .367.774v6.764a1 1 0 0 1-1 1H9v-4H7v4H3a1 1 0 0 1-1-1V6.237a1 1 0 0 1 .367-.774l5-4.09ZM8 2.146 3 6.237v6.764h3v-3a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3h3V6.237l-5-4.09Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 16.25V7.705L10 2.497 3.75 7.705v8.545H7.5v-4.125a1.5 1.5 0 0 1 1.5-1.5h2a1.5 1.5 0 0 1 1.5 1.5v4.125h3.75ZM2.95 6.745a1.25 1.25 0 0 0-.45.96v8.545c0 .69.56 1.25 1.25 1.25h5v-5.375a.25.25 0 0 1 .25-.25h2a.25.25 0 0 1 .25.25V17.5h5c.69 0 1.25-.56 1.25-1.25V7.705a1.25 1.25 0 0 0-.45-.96L10.8 1.536a1.25 1.25 0 0 0-1.6 0L2.95 6.745Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 19.5V9.245L12 2.996l-7.5 6.25v10.253H9v-4.95a1.8 1.8 0 0 1 1.8-1.8h2.4a1.8 1.8 0 0 1 1.8 1.8v4.95h4.5ZM3.54 8.092A1.5 1.5 0 0 0 3 9.246v10.253A1.5 1.5 0 0 0 4.5 21h6v-6.45a.3.3 0 0 1 .3-.3h2.4a.3.3 0 0 1 .3.3V21h6a1.5 1.5 0 0 0 1.5-1.5V9.246a1.5 1.5 0 0 0-.54-1.153l-7.5-6.25a1.5 1.5 0 0 0-1.92 0l-7.5 6.25Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 26V12.327L16 3.994 6 12.328v13.671h6v-6.6a2.4 2.4 0 0 1 2.4-2.4h3.2a2.4 2.4 0 0 1 2.4 2.4V26h6ZM4.72 10.79A2 2 0 0 0 4 12.328v13.671a2 2 0 0 0 2 2h8v-8.6c0-.22.18-.4.4-.4h3.2c.22 0 .4.18.4.4V28h8a2 2 0 0 0 2-2V12.328a2 2 0 0 0-.72-1.537l-10-8.333a2 2 0 0 0-2.56 0l-10 8.333Z";
}