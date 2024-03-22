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

public class UserCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 14a5.98 5.98 0 0 0 4.208-1.723L11.7 11.6a1.5 1.5 0 0 0-1.2-.6h-5a1.5 1.5 0 0 0-1.2.6l-.508.677A5.98 5.98 0 0 0 8 14Zm4.5-3 .374.5a6 6 0 1 0-9.749 0L3.5 11a2.5 2.5 0 0 1 2-1h5a2.5 2.5 0 0 1 2 1ZM8 15A7 7 0 1 0 8 1a7 7 0 0 0 0 14Zm1.5-8.5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0Zm1 0a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Z";

    protected override string IconGeometry20 { get; }
        = "M10 17.5a7.476 7.476 0 0 0 5.26-2.154l-.635-.846a1.875 1.875 0 0 0-1.5-.75h-6.25c-.59 0-1.146.278-1.5.75l-.635.846A7.476 7.476 0 0 0 10 17.5Zm5.625-3.75.468.624a7.5 7.5 0 1 0-12.186 0l.468-.624a3.125 3.125 0 0 1 2.5-1.25h6.25c.984 0 1.91.463 2.5 1.25Zm-5.625 5a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5Zm1.875-10.625a1.875 1.875 0 1 1-3.75 0 1.875 1.875 0 0 1 3.75 0Zm1.25 0a3.125 3.125 0 1 1-6.25 0 3.125 3.125 0 0 1 6.25 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 21a8.97 8.97 0 0 0 6.312-2.584L17.55 17.4a2.25 2.25 0 0 0-1.8-.9h-7.5a2.25 2.25 0 0 0-1.8.9l-.762 1.016A8.97 8.97 0 0 0 12 21Zm6.75-4.5.562.749a9 9 0 1 0-14.623 0l.561-.749a3.75 3.75 0 0 1 3-1.5h7.5a3.75 3.75 0 0 1 3 1.5Zm-6.75 6c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5Zm2.25-12.75a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Zm1.5 0a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 28c3.279 0 6.25-1.315 8.416-3.446L23.4 23.2A3 3 0 0 0 21 22H11a3 3 0 0 0-2.4 1.2l-1.016 1.354A11.961 11.961 0 0 0 16 28Zm9-6 .749.999A11.946 11.946 0 0 0 28 16c0-6.627-5.373-12-12-12S4 9.373 4 16c0 2.612.834 5.029 2.251 6.999L7 22a5 5 0 0 1 4-2h10a5 5 0 0 1 4 2Zm-9 8c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Zm3-17a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm2 0a5 5 0 1 1-10 0 5 5 0 0 1 10 0Z";
}