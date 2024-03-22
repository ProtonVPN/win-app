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

public class TrashClock : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M12 10a.5.5 0 0 0-1 0v1.5a.5.5 0 0 0 .146.354l1 1a.5.5 0 0 0 .708-.708L12 11.293V10Z M5 0a.5.5 0 0 0-.447.276L3.69 2H.5a.5.5 0 0 0 0 1h1.272l.435 9.568A1.5 1.5 0 0 0 3.706 14H7.75v-.012a4.5 4.5 0 1 0 3.525-6.983l.2-4.005H12.5a.5.5 0 0 0 0-1H9.309L8.447.276A.5.5 0 0 0 8 0H5Zm2 11.5c0 .526.09 1.03.256 1.5h-3.55a.5.5 0 0 1-.5-.477L2.773 3h7.701l-.208 4.171A4.502 4.502 0 0 0 7 11.5ZM8.191 2l-.5-1H5.309l-.5 1h3.382ZM11.5 8a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7Z";

    protected override string IconGeometry20 { get; }
        = "M15 12.5a.625.625 0 1 0-1.25 0v1.875c0 .166.066.325.183.442l1.25 1.25a.625.625 0 1 0 .884-.884L15 14.116V12.5Z M6.25 0a.625.625 0 0 0-.559.345L4.614 2.5H.625a.625.625 0 1 0 0 1.25h1.59l.544 11.96a1.875 1.875 0 0 0 1.873 1.79h5.056v-.015a5.625 5.625 0 1 0 4.407-8.729l.25-5.006h1.28a.625.625 0 1 0 0-1.25h-3.989L10.56.345A.625.625 0 0 0 10 0H6.25Zm2.5 14.375c0 .657.113 1.289.32 1.875H4.632a.625.625 0 0 1-.624-.597L3.467 3.75h9.626l-.26 5.214a5.627 5.627 0 0 0-4.083 5.411ZM10.239 2.5l-.625-1.25H6.636L6.011 2.5h4.228Zm4.136 7.5a4.375 4.375 0 1 0 0 8.75 4.375 4.375 0 0 0 0-8.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M18 15a.75.75 0 0 0-1.5 0v2.25c0 .199.079.39.22.53l1.5 1.5a.75.75 0 1 0 1.06-1.06L18 16.94V15Z M7.5 0a.75.75 0 0 0-.67.415L5.535 3H.75a.75.75 0 0 0 0 1.5h1.908l.653 14.352A2.25 2.25 0 0 0 5.558 21h6.067v-.017a6.75 6.75 0 1 0 5.288-10.474l.3-6.009h1.537a.75.75 0 0 0 0-1.5h-4.787L12.671.415A.75.75 0 0 0 12 0H7.5Zm3 17.25c0 .789.135 1.546.384 2.25H5.558a.75.75 0 0 1-.749-.716L4.16 4.5h11.553l-.313 6.257a6.753 6.753 0 0 0-4.9 6.493ZM12.287 3l-.75-1.5H7.963L7.214 3h5.072Zm4.963 9a5.25 5.25 0 1 0 0 10.5 5.25 5.25 0 0 0 0-10.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M24 20a1 1 0 1 0-2 0v3a1 1 0 0 0 .293.707l2 2a1 1 0 0 0 1.414-1.414L24 22.586V20Z M10 0a1 1 0 0 0-.894.553L7.382 4H1a1 1 0 0 0 0 2h2.544l.87 19.136A3 3 0 0 0 7.411 28H15.5v-.023a9 9 0 1 0 7.05-13.966L22.952 6H25a1 1 0 1 0 0-2h-6.382L16.894.553A1 1 0 0 0 16 0h-6Zm4 23c0 1.052.18 2.062.512 3h-7.1a1 1 0 0 1-1-.955L5.546 6H20.95l-.417 8.343A9.004 9.004 0 0 0 14 23Zm2.382-19-1-2h-4.764l-1 2h6.764ZM23 16a7 7 0 1 0 0 14 7 7 0 0 0 0-14Z";
}