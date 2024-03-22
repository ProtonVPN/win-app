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

public class QuestionCircleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 16a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm0-10.852a1.5 1.5 0 0 0-1.5 1.5.5.5 0 0 1-1 0 2.5 2.5 0 0 1 2.5-2.5h.148A2.352 2.352 0 0 1 11 6.5c0 .566-.266 1.098-.719 1.437l-.681.511a1.5 1.5 0 0 0-.6 1.2.5.5 0 0 1-1 0 2.5 2.5 0 0 1 1-2l.681-.51A.8.8 0 0 0 10 6.5c0-.746-.605-1.352-1.352-1.352H8.5Zm0 7.602a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 20a9.375 9.375 0 1 0 0-18.75 9.375 9.375 0 1 0 0 18.75Zm0-13.564A1.874 1.874 0 0 0 8.75 8.311a.625.625 0 0 1-1.25 0c0-1.726 1.4-3.125 3.125-3.125h.185a2.94 2.94 0 0 1 2.94 2.94c0 .706-.333 1.372-.898 1.796L12 10.56c-.472.355-.75.91-.75 1.5a.625.625 0 1 1-1.25 0c0-.983.463-1.91 1.25-2.5l.852-.638a.995.995 0 0 0 .398-.797 1.69 1.69 0 0 0-1.69-1.69h-.185Zm0 9.502a.937.937 0 1 0 0-1.875.937.937 0 0 0 0 1.874Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 24C18.963 24 24 18.963 24 12.75S18.963 1.5 12.75 1.5 1.5 6.537 1.5 12.75 6.537 24 12.75 24Zm0-16.277a2.25 2.25 0 0 0-2.25 2.25.75.75 0 1 1-1.5 0 3.75 3.75 0 0 1 3.75-3.75h.222A3.528 3.528 0 0 1 16.5 9.75c0 .848-.4 1.647-1.078 2.156l-1.022.767a2.25 2.25 0 0 0-.9 1.8.75.75 0 0 1-1.5 0 3.75 3.75 0 0 1 1.5-3l1.022-.767c.301-.226.478-.58.478-.956 0-1.12-.908-2.027-2.028-2.027h-.222Zm0 11.402a1.125 1.125 0 1 0 0-2.25 1.125 1.125 0 0 0 0 2.25Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 32c8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15Zm0-21.703a3 3 0 0 0-3 3 1 1 0 1 1-2 0 5 5 0 0 1 5-5h.296A4.704 4.704 0 0 1 22 13c0 1.13-.532 2.195-1.437 2.874L19.2 16.897a3 3 0 0 0-1.2 2.4 1 1 0 1 1-2 0 5 5 0 0 1 2-4l1.363-1.022c.401-.301.637-.773.637-1.274a2.704 2.704 0 0 0-2.704-2.704H17ZM17 25.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z";
}