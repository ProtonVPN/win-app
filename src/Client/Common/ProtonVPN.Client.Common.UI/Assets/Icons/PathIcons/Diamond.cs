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

public class Diamond : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14 1.5c0-.276-.5-1-.5-1s-.5.724-.5 1V2h-.5c-.276 0-1 .5-1 .5s.724.5 1 .5h.5v.5c0 .276.5 1 .5 1s.5-.724.5-1V3h.5c.276 0 1-.5 1-.5s-.724-.5-1-.5H14v-.5ZM3.5 3a.5.5 0 0 0-.312.11l-2.5 2a.5.5 0 0 0-.064.72l7 8a.5.5 0 0 0 .752 0l7-8A.5.5 0 0 0 15 5H2.425l1.25-1H10.5a.5.5 0 0 0 0-1h-7Zm1.154 3H2.102l4.465 5.103L4.654 6Zm9.244 0-4.465 5.103L11.347 6h2.551ZM5.721 6h4.557L8 12.076 5.721 6Z";

    protected override string IconGeometry20 { get; }
        = "M17.5 3.125c0-.345-.625-1.25-.625-1.25s-.625.905-.625 1.25v.625h-.625c-.345 0-1.25.625-1.25.625S15.28 5 15.625 5h.625v.625c0 .345.625 1.25.625 1.25s.625-.905.625-1.25V5h.625c.345 0 1.25-.625 1.25-.625s-.905-.625-1.25-.625H17.5v-.625ZM4.375 5a.625.625 0 0 0-.39.137L.86 7.637a.625.625 0 0 0-.08.9l8.75 10a.625.625 0 0 0 .94 0l8.75-10a.625.625 0 0 0-.47-1.037H3.032l1.562-1.25h8.531a.625.625 0 0 0 0-1.25h-8.75Zm1.442 3.75h-3.19l5.582 6.379L5.817 8.75Zm11.556 0-5.582 6.379 2.392-6.379h3.19Zm-10.221 0h5.696L10 16.345 7.152 8.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M21 3.75c0-.414-.75-1.5-.75-1.5s-.75 1.086-.75 1.5v.75h-.75c-.414 0-1.5.75-1.5.75s1.086.75 1.5.75h.75v.75c0 .414.75 1.5.75 1.5s.75-1.086.75-1.5V6h.75c.414 0 1.5-.75 1.5-.75s-1.086-.75-1.5-.75H21v-.75ZM5.25 6a.75.75 0 0 0-.469.164l-3.75 3a.75.75 0 0 0-.095 1.08l10.5 12a.75.75 0 0 0 1.128 0l10.5-12A.75.75 0 0 0 22.5 9H3.638l1.875-1.5H15.75a.75.75 0 0 0 0-1.5H5.25Zm1.73 4.5H3.153l6.698 7.655L6.98 10.5Zm13.867 0-6.698 7.655 2.87-7.655h3.828Zm-12.265 0h6.836L12 19.614 8.582 10.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M28 5c0-.553-1-2-1-2s-1 1.447-1 2v1h-1c-.552 0-2 1-2 1s1.448 1 2 1h1v1c0 .552 1 2 1 2s1-1.448 1-2V8h1c.552 0 2-1 2-1s-1.448-1-2-1h-1V5ZM7 8a1 1 0 0 0-.625.22l-5 4a1 1 0 0 0-.128 1.438l14 16a1 1 0 0 0 1.506 0l14-16A1 1 0 0 0 30 12H4.85l2.5-2H21a1 1 0 0 0 0-2H7Zm2.307 6H4.204l8.93 10.206L9.307 14Zm18.49 0-8.931 10.206L22.693 14h5.103Zm-16.354 0h9.114L16 26.152 11.443 14Z";
}