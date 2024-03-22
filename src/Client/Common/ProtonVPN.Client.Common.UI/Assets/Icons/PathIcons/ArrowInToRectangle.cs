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

public class ArrowInToRectangle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7 1h6a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-1h1v1a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V3a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1v1H5V3a2 2 0 0 1 2-2Zm-.354 4.646a.5.5 0 0 1 .708 0l2.358 2.359a.7.7 0 0 1 0 .99l-2.358 2.359a.5.5 0 0 1-.708-.708L8.293 9H1.5a.5.5 0 0 1 0-1h6.793L6.646 6.354a.5.5 0 0 1 0-.708Z";

    protected override string IconGeometry20 { get; }
        = "M9.375 1.25h6.875a2.5 2.5 0 0 1 2.5 2.5v12.5a2.5 2.5 0 0 1-2.5 2.5H9.375a2.5 2.5 0 0 1-2.5-2.5V15h1.25v1.25c0 .69.56 1.25 1.25 1.25h6.875c.69 0 1.25-.56 1.25-1.25V3.75c0-.69-.56-1.25-1.25-1.25H9.375c-.69 0-1.25.56-1.25 1.25V5h-1.25V3.75a2.5 2.5 0 0 1 2.5-2.5ZM8.308 6.433a.625.625 0 0 1 .884 0l2.948 2.948a.874.874 0 0 1 0 1.238l-2.948 2.948a.625.625 0 1 1-.884-.884l2.058-2.058H1.875a.625.625 0 1 1 0-1.25h8.491L8.308 7.317a.625.625 0 0 1 0-.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.25 1.5h8.25a3 3 0 0 1 3 3v15a3 3 0 0 1-3 3h-8.25a3 3 0 0 1-3-3V18h1.5v1.5a1.5 1.5 0 0 0 1.5 1.5h8.25a1.5 1.5 0 0 0 1.5-1.5v-15A1.5 1.5 0 0 0 19.5 3h-8.25a1.5 1.5 0 0 0-1.5 1.5V6h-1.5V4.5a3 3 0 0 1 3-3ZM9.97 7.72a.75.75 0 0 1 1.06 0l3.538 3.538a1.049 1.049 0 0 1 0 1.485L11.03 16.28a.75.75 0 1 1-1.06-1.06l2.47-2.47H2.25a.75.75 0 0 1 0-1.5h10.19L9.97 8.78a.75.75 0 0 1 0-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M15 2h11a4 4 0 0 1 4 4v20a4 4 0 0 1-4 4H15a4 4 0 0 1-4-4v-2h2v2a2 2 0 0 0 2 2h11a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2H15a2 2 0 0 0-2 2v2h-2V6a4 4 0 0 1 4-4Zm-1.707 8.293a1 1 0 0 1 1.414 0l4.717 4.717a1.399 1.399 0 0 1 0 1.98l-4.717 4.717a1 1 0 0 1-1.414-1.414L16.586 17H3a1 1 0 1 1 0-2h13.586l-3.293-3.293a1 1 0 0 1 0-1.414Z";
}