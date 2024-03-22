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

public class FileShapes : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 13a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1h5v2.5A1.5 1.5 0 0 0 10.5 6H13v7Zm-.414-8L10 2.414V4.5a.5.5 0 0 0 .5.5h2.086ZM2 3a2 2 0 0 1 2-2h5.172a2 2 0 0 1 1.414.586l2.828 2.828A2 2 0 0 1 14 5.828V13a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V3Zm3 8v1h1v-1H5Zm-.5-1a.5.5 0 0 0-.5.5v2a.5.5 0 0 0 .5.5h2a.5.5 0 0 0 .5-.5v-2a.5.5 0 0 0-.5-.5h-2Z M9.5 9a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Zm0 1a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 16.25c0 .69-.56 1.25-1.25 1.25H5c-.69 0-1.25-.56-1.25-1.25V3.75c0-.69.56-1.25 1.25-1.25h6.25v3.125c0 1.036.84 1.875 1.875 1.875h3.125v8.75Zm-.518-10L12.5 3.018v2.607c0 .345.28.625.625.625h2.607ZM2.5 3.75A2.5 2.5 0 0 1 5 1.25h6.464a2.5 2.5 0 0 1 1.768.732l3.536 3.536a2.5 2.5 0 0 1 .732 1.768v8.964a2.5 2.5 0 0 1-2.5 2.5H5a2.5 2.5 0 0 1-2.5-2.5V3.75Zm3.75 10V15H7.5v-1.25H6.25Zm-.625-1.25a.625.625 0 0 0-.625.625v2.5c0 .345.28.625.625.625h2.5c.345 0 .625-.28.625-.625v-2.5a.625.625 0 0 0-.625-.625h-2.5Z M11.875 11.25a.625.625 0 1 0 0-1.25.625.625 0 0 0 0 1.25Zm0 1.25a1.875 1.875 0 1 0 0-3.75 1.875 1.875 0 0 0 0 3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 19.5A1.5 1.5 0 0 1 18 21H6a1.5 1.5 0 0 1-1.5-1.5v-15A1.5 1.5 0 0 1 6 3h7.5v3.75A2.25 2.25 0 0 0 15.75 9h3.75v10.5Zm-.621-12L15 3.621V6.75c0 .414.336.75.75.75h3.129ZM3 4.5a3 3 0 0 1 3-3h7.757a3 3 0 0 1 2.122.879L20.12 6.62A3 3 0 0 1 21 8.743V19.5a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3v-15Zm4.5 12V18H9v-1.5H7.5ZM6.75 15a.75.75 0 0 0-.75.75v3c0 .414.336.75.75.75h3a.75.75 0 0 0 .75-.75v-3a.75.75 0 0 0-.75-.75h-3Z M14.25 13.5a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Zm0 1.5a2.25 2.25 0 1 0 0-4.5 2.25 2.25 0 0 0 0 4.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 26a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h10v5a3 3 0 0 0 3 3h5v14Zm-.828-16L20 4.828V9a1 1 0 0 0 1 1h4.172ZM4 6a4 4 0 0 1 4-4h10.343a4 4 0 0 1 2.829 1.172l5.656 5.656A4 4 0 0 1 28 11.657V26a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4V6Zm6 16v2h2v-2h-2Zm-1-2a1 1 0 0 0-1 1v4a1 1 0 0 0 1 1h4a1 1 0 0 0 1-1v-4a1 1 0 0 0-1-1H9Z M19 18a1 1 0 1 0 0-2 1 1 0 0 0 0 2Zm0 2a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z";
}