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

public class Cloud : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M9.888 6 9.6 5.5A3 3 0 0 0 4 7v.878l-.666.236A2.001 2.001 0 0 0 4 12h7a3 3 0 1 0 0-6H9.888Zm.577-1H11a4 4 0 0 1 0 8H4a3 3 0 0 1-1-5.83V7a4 4 0 0 1 7.465-2Z";

    protected override string IconGeometry20 { get; }
        = "M12.36 7.5 12 6.876A3.75 3.75 0 0 0 5 8.75v1.097l-.833.295A2.502 2.502 0 0 0 5 15h8.75a3.75 3.75 0 1 0 0-7.5h-1.39Zm.721-1.25h.669a5 5 0 0 1 0 10H5a3.75 3.75 0 0 1-1.25-7.287V8.75a5 5 0 0 1 9.331-2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "m14.832 9-.433-.749A4.5 4.5 0 0 0 6 10.5v1.317l-1 .353A3.002 3.002 0 0 0 6 18h10.5a4.5 4.5 0 1 0 0-9h-1.668Zm.865-1.5h.803a6 6 0 0 1 0 12H6a4.5 4.5 0 0 1-1.5-8.744V10.5a6 6 0 0 1 11.197-3Z"; 

    protected override string IconGeometry32 { get; }
        = "m19.776 12-.578-.998A6 6 0 0 0 8 14v1.756l-1.334.471A4 4 0 0 0 8 24h14a6 6 0 0 0 0-12h-2.224Zm1.154-2H22a8 8 0 1 1 0 16H8a6 6 0 0 1-2-11.659V14a8 8 0 0 1 14.93-4Z";
}