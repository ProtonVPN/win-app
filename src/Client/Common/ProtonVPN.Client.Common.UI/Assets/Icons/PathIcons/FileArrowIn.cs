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

public class FileArrowIn : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.5 2A1.5 1.5 0 0 0 3 3.5v9A1.5 1.5 0 0 0 4.5 14H9v1H4.5A2.5 2.5 0 0 1 2 12.5v-9A2.5 2.5 0 0 1 4.5 1h4.672a2.5 2.5 0 0 1 1.767.732l2.329 2.329A2.5 2.5 0 0 1 14 5.828V10h-1V6h-2.5A1.5 1.5 0 0 1 9 4.5V2H4.5Zm8.06 2.768c.072.071.135.149.19.232H10.5a.5.5 0 0 1-.5-.5V2.25c.083.054.16.118.232.19l2.329 2.328Zm-.707 4.378a.5.5 0 0 0-.707 0l-1.858 1.859a.7.7 0 0 0 0 .99l1.858 1.859a.5.5 0 0 0 .707-.708L10.707 12H15a.5.5 0 0 0 0-1h-4.293l1.146-1.146a.5.5 0 0 0 0-.708Z";

    protected override string IconGeometry20 { get; }
        = "M5.625 2.5c-1.036 0-1.875.84-1.875 1.875v11.25c0 1.035.84 1.875 1.875 1.875h5.625v1.25H5.625A3.125 3.125 0 0 1 2.5 15.625V4.375c0-1.726 1.4-3.125 3.125-3.125h5.84c.828 0 1.623.33 2.21.915l2.91 2.91c.586.587.915 1.382.915 2.21V12.5h-1.25v-5h-3.125a1.875 1.875 0 0 1-1.875-1.875V2.5H5.625Zm10.076 3.46c.09.089.168.186.237.29h-2.813a.625.625 0 0 1-.625-.625V2.812c.104.069.201.148.29.237l2.91 2.91Zm-.884 5.473a.625.625 0 0 0-.884 0l-2.323 2.323a.875.875 0 0 0 0 1.238l2.323 2.323a.625.625 0 0 0 .884-.884L13.384 15h5.366a.625.625 0 1 0 0-1.25h-5.366l1.433-1.433a.625.625 0 0 0 0-.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M6.75 3A2.25 2.25 0 0 0 4.5 5.25v13.5A2.25 2.25 0 0 0 6.75 21h6.75v1.5H6.75A3.75 3.75 0 0 1 3 18.75V5.25A3.75 3.75 0 0 1 6.75 1.5h7.007a3.75 3.75 0 0 1 2.652 1.098l3.493 3.493A3.75 3.75 0 0 1 21 8.743V15h-1.5V9h-3.75a2.25 2.25 0 0 1-2.25-2.25V3H6.75Zm12.091 4.152c.107.107.202.224.285.348H15.75a.75.75 0 0 1-.75-.75V3.374c.124.083.241.178.348.285l3.493 3.493Zm-1.06 6.568a.75.75 0 0 0-1.061 0l-2.788 2.787a1.05 1.05 0 0 0 0 1.485l2.788 2.788a.75.75 0 0 0 1.06-1.06L16.06 18h6.44a.75.75 0 0 0 0-1.5h-6.44l1.72-1.72a.75.75 0 0 0 0-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M9 4a3 3 0 0 0-3 3v18a3 3 0 0 0 3 3h9v2H9a5 5 0 0 1-5-5V7a5 5 0 0 1 5-5h9.343a5 5 0 0 1 3.536 1.464l4.656 4.657A5 5 0 0 1 28 11.657V20h-2v-8h-5a3 3 0 0 1-3-3V4H9Zm16.121 5.536a3 3 0 0 1 .38.464H21a1 1 0 0 1-1-1V4.499c.166.11.322.237.465.38l4.656 4.657Zm-1.414 8.757a1 1 0 0 0-1.414 0l-3.717 3.717a1.4 1.4 0 0 0 0 1.98l3.717 3.717a1 1 0 0 0 1.414-1.414L21.414 24H30a1 1 0 0 0 0-2h-8.586l2.293-2.293a1 1 0 0 0 0-1.414Z";
}