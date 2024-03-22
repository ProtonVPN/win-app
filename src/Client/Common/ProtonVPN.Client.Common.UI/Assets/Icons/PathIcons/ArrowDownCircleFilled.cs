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

public class ArrowDownCircleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 16a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm0-11.712a.5.5 0 0 1 .5.5v5.793l1.646-1.647a.5.5 0 0 1 .708.707L8.995 12a.7.7 0 0 1-.99 0L5.646 9.641a.5.5 0 1 1 .708-.707L8 10.581V4.788a.5.5 0 0 1 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 20a9.375 9.375 0 1 0 0-18.75 9.375 9.375 0 1 0 0 18.75Zm0-14.64c.345 0 .625.28.625.625v7.241l2.058-2.058a.625.625 0 1 1 .884.884L11.244 15a.875.875 0 0 1-1.238 0l-2.948-2.948a.625.625 0 0 1 .884-.884L10 13.226V5.985c0-.345.28-.625.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 24C18.963 24 24 18.963 24 12.75S18.963 1.5 12.75 1.5 1.5 6.537 1.5 12.75 6.537 24 12.75 24Zm0-17.568a.75.75 0 0 1 .75.75v8.69l2.47-2.47a.75.75 0 1 1 1.06 1.06L13.492 18a1.05 1.05 0 0 1-1.485 0L8.47 14.462a.75.75 0 1 1 1.06-1.06l2.47 2.47v-8.69a.75.75 0 0 1 .75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 32c8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15Zm0-23.424a1 1 0 0 1 1 1V21.16l3.293-3.292a1 1 0 0 1 1.414 1.414L17.99 24a1.4 1.4 0 0 1-1.98 0l-4.717-4.717a1 1 0 0 1 1.414-1.414L16 21.162V9.575a1 1 0 0 1 1-1Z";
}