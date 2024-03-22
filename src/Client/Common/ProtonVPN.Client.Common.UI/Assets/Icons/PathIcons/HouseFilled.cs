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

public class HouseFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.633 1.373a1 1 0 0 0-1.266 0l-5 4.09A1 1 0 0 0 2 6.237v6.764a1 1 0 0 0 1 1h4v-3.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5V14h4a1 1 0 0 0 1-1V6.237a1 1 0 0 0-.367-.774l-5-4.09Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 7.862c0-.38.165-.74.45-.983L9.2 1.547a1.228 1.228 0 0 1 1.6 0l6.25 5.332c.285.243.45.603.45.983v8.358c0 .707-.56 1.28-1.25 1.28h-4.375v-5.503a.253.253 0 0 0-.25-.256h-3.25a.253.253 0 0 0-.25.256V17.5H3.75c-.69 0-1.25-.573-1.25-1.28V7.862Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 9.434c0-.456.198-.888.54-1.18l7.5-6.398a1.473 1.473 0 0 1 1.92 0l7.5 6.398c.342.292.54.724.54 1.18v10.03c0 .849-.672 1.536-1.5 1.536h-5.25v-6.603a.304.304 0 0 0-.3-.307h-3.9c-.166 0-.3.137-.3.307V21H4.5c-.828 0-1.5-.688-1.5-1.536V9.434Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 12.579c0-.608.264-1.184.72-1.573l10-8.531a1.965 1.965 0 0 1 2.56 0l10 8.53c.456.39.72.966.72 1.574v13.373C28 27.084 27.105 28 26 28h-7v-8.804c0-.227-.18-.41-.4-.41h-5.2c-.22 0-.4.183-.4.41V28H6c-1.105 0-2-.917-2-2.047V12.579Z";
}