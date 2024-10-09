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

public class ArrowWithinSquare : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M12 4.724V8.5a.5.5 0 0 1-1 0V5.707l-5.646 5.647a.5.5 0 0 1-.708-.708L10.293 5H7.5a.5.5 0 1 1 0-1h3.776c.042 0 .1 0 .157.005a.5.5 0 0 1 .563.562c.004.056.004.116.004.157Z M3 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V3a2 2 0 0 0-2-2H3Zm10 1H3a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V3a1 1 0 0 0-1-1Z";

    protected override string IconGeometry20 { get; }
        = "M15 5.906v4.719a.625.625 0 1 1-1.25 0V7.134l-7.058 7.058a.625.625 0 1 1-.884-.884l7.058-7.058H9.375a.625.625 0 1 1 0-1.25h4.72c.052 0 .126 0 .196.006a.624.624 0 0 1 .703.703c.006.07.006.145.006.197Z M3.75 1.25a2.5 2.5 0 0 0-2.5 2.5v12.5a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5V3.75a2.5 2.5 0 0 0-2.5-2.5H3.75Zm12.5 1.25H3.75c-.69 0-1.25.56-1.25 1.25v12.5c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25V3.75c0-.69-.56-1.25-1.25-1.25Z";

    protected override string IconGeometry24 { get; }
        = "M18 7.087v5.663a.75.75 0 0 1-1.5 0V8.56l-8.47 8.47a.75.75 0 0 1-1.06-1.06l8.47-8.47h-4.19a.75.75 0 0 1 0-1.5h5.664c.062 0 .151 0 .235.007a.749.749 0 0 1 .844.844c.007.084.007.173.007.236Z M4.5 1.5a3 3 0 0 0-3 3v15a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3v-15a3 3 0 0 0-3-3h-15Zm15 1.5h-15A1.5 1.5 0 0 0 3 4.5v15A1.5 1.5 0 0 0 4.5 21h15a1.5 1.5 0 0 0 1.5-1.5v-15A1.5 1.5 0 0 0 19.5 3Z";

    protected override string IconGeometry32 { get; }
        = "M24 9.449V17a1 1 0 1 1-2 0v-5.586L10.707 22.707a1 1 0 0 1-1.414-1.414L20.586 10H15a1 1 0 1 1 0-2h7.552c.083 0 .202 0 .314.009a.998.998 0 0 1 1.125 1.126c.01.111.01.23.009.314Z M6 2a4 4 0 0 0-4 4v20a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4V6a4 4 0 0 0-4-4H6Zm20 2H6a2 2 0 0 0-2 2v20a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2Z";
}
