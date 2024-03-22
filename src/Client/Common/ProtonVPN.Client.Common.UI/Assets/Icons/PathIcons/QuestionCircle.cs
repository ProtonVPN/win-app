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

public class QuestionCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7 6.648a1.5 1.5 0 0 1 1.5-1.5h.148C9.395 5.148 10 5.754 10 6.5c0 .25-.118.487-.319.637L9 7.648a2.5 2.5 0 0 0-1 2 .5.5 0 0 0 1 0 1.5 1.5 0 0 1 .6-1.2l.681-.51c.453-.34.719-.872.719-1.438a2.352 2.352 0 0 0-2.352-2.352H8.5a2.5 2.5 0 0 0-2.5 2.5.5.5 0 0 0 1 0Zm1.5 6.102a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Z M1 8.5a7.5 7.5 0 1 1 15 0 7.5 7.5 0 0 1-15 0ZM8.5 2a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13Z";

    protected override string IconGeometry20 { get; }
        = "M8.75 8.31c0-1.035.84-1.874 1.875-1.874h.185c.933 0 1.69.756 1.69 1.69a.995.995 0 0 1-.398.796l-.852.639a3.125 3.125 0 0 0-1.25 2.5.625.625 0 1 0 1.25 0c0-.59.278-1.146.75-1.5l.852-.64a2.244 2.244 0 0 0 .898-1.796 2.94 2.94 0 0 0-2.94-2.94h-.185A3.125 3.125 0 0 0 7.5 8.312a.625.625 0 0 0 1.25 0Zm1.875 7.628a.937.937 0 1 0 0-1.875.937.937 0 0 0 0 1.874Z M1.25 10.625a9.375 9.375 0 1 1 18.75 0 9.375 9.375 0 0 1-18.75 0ZM10.625 2.5a8.125 8.125 0 1 0 0 16.25 8.125 8.125 0 0 0 0-16.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.5 9.973a2.25 2.25 0 0 1 2.25-2.25h.222c1.12 0 2.028.908 2.028 2.027 0 .376-.177.73-.478.956l-1.022.767a3.75 3.75 0 0 0-1.5 3 .75.75 0 0 0 1.5 0c0-.709.333-1.375.9-1.8l1.022-.767A2.695 2.695 0 0 0 16.5 9.75a3.528 3.528 0 0 0-3.528-3.527h-.222A3.75 3.75 0 0 0 9 9.973a.75.75 0 1 0 1.5 0Zm2.25 9.152a1.125 1.125 0 1 0 0-2.25 1.125 1.125 0 0 0 0 2.25Z M1.5 12.75C1.5 6.537 6.537 1.5 12.75 1.5S24 6.537 24 12.75 18.963 24 12.75 24 1.5 18.963 1.5 12.75ZM12.75 3C7.365 3 3 7.365 3 12.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S18.135 3 12.75 3Z"; 

    protected override string IconGeometry32 { get; }
        = "M14 13.297a3 3 0 0 1 3-3h.296A2.704 2.704 0 0 1 20 13c0 .5-.236.973-.637 1.274L18 15.297a5 5 0 0 0-2 4 1 1 0 1 0 2 0 3 3 0 0 1 1.2-2.4l1.363-1.022A3.593 3.593 0 0 0 22 13a4.704 4.704 0 0 0-4.704-4.704H17a5 5 0 0 0-5 5 1 1 0 1 0 2 0ZM17 25.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z M2 17C2 8.716 8.716 2 17 2c8.284 0 15 6.716 15 15 0 8.284-6.716 15-15 15-8.284 0-15-6.716-15-15ZM17 4C9.82 4 4 9.82 4 17s5.82 13 13 13 13-5.82 13-13S24.18 4 17 4Z";
}