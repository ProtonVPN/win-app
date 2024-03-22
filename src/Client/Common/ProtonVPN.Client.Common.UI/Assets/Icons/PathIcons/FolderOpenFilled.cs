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

public class FolderOpenFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.5 2A1.5 1.5 0 0 0 1 3.5v8.536a1.54 1.54 0 0 1 .085-.207l2.5-5A1.5 1.5 0 0 1 4.927 6H14V4.5A1.5 1.5 0 0 0 12.5 3H7.618l-1.683-.842A1.5 1.5 0 0 0 5.264 2H2.5Z M1.98 12.276a.512.512 0 0 0-.018.041l-.107.187A1 1 0 0 0 2.723 14h9.697a1 1 0 0 0 .868-.504l3.07-5.374A.75.75 0 0 0 15.709 7H4.928a.5.5 0 0 0-.448.276l-2.5 5Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 11.25v-7.5c0-.69.56-1.25 1.25-1.25h3.926c.29 0 .571.102.795.286l2.085 1.72a.25.25 0 0 0 .159.057h6.785c.69 0 1.25.56 1.25 1.25v1.062H6.745c-.69 0-1.324.379-1.651.986l-3.844 7.14v-3.75Zm6.125-3.125c-.692 0-1.329.382-1.654.993l-3.49 6.544A1.25 1.25 0 0 0 3.332 17.5h11.542a1.25 1.25 0 0 0 1.103-.662l3.912-7.334a.937.937 0 0 0-.828-1.38H7.376Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 13.5v-9A1.5 1.5 0 0 1 3 3h4.711a1.5 1.5 0 0 1 .955.344l2.5 2.063a.3.3 0 0 0 .192.069H19.5a1.5 1.5 0 0 1 1.5 1.5V8.25H8.094a2.25 2.25 0 0 0-1.981 1.184L1.5 18v-4.5Zm7.35-3.75a2.25 2.25 0 0 0-1.985 1.191l-4.189 7.853A1.5 1.5 0 0 0 4 21h13.85a1.5 1.5 0 0 0 1.323-.794l4.695-8.802a1.125 1.125 0 0 0-.993-1.654H8.85Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 18V6.001a2 2 0 0 1 2-2h6.281a2 2 0 0 1 1.273.457L14.89 7.21a.4.4 0 0 0 .255.091H26a2 2 0 0 1 2 2v1.7H10.792a3 3 0 0 0-2.642 1.577L2 24.001v-6Zm9.8-5a3 3 0 0 0-2.647 1.588l-5.584 10.47C2.859 26.392 3.823 28 5.333 28H23.8a2 2 0 0 0 1.765-1.059l6.258-11.735A1.5 1.5 0 0 0 30.5 13H11.8Z";
}