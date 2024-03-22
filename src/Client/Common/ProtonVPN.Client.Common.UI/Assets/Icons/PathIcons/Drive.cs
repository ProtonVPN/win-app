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

public class Drive : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14 12v-2a1 1 0 0 0-1-1H3a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1Zm1 0V9.41a2 2 0 0 0-.162-.787l-2.318-5.41A2 2 0 0 0 10.68 2H5.32a2 2 0 0 0-1.84 1.212l-2.32 5.41a2 2 0 0 0-.16.79V12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2Zm-3.4-8.394 1.912 4.46A2.004 2.004 0 0 0 13 8H3c-.177 0-.348.023-.512.066L4.4 3.606A1 1 0 0 1 5.319 3h5.362a1 1 0 0 1 .92.606Zm.9 7.894a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Z";

    protected override string IconGeometry20 { get; }
        = "M17.5 15v-2.5c0-.69-.56-1.25-1.25-1.25H3.75c-.69 0-1.25.56-1.25 1.25V15c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25Zm1.25 0v-3.237c0-.338-.069-.674-.202-.985l-2.899-6.763A2.5 2.5 0 0 0 13.352 2.5H6.647A2.5 2.5 0 0 0 4.35 4.015l-2.899 6.763a2.5 2.5 0 0 0-.202.985V15a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5ZM14.5 4.508l2.39 5.575a2.503 2.503 0 0 0-.64-.083H3.75a2.5 2.5 0 0 0-.64.083L5.5 4.508a1.25 1.25 0 0 1 1.148-.758h6.704c.5 0 .951.298 1.148.758Zm1.125 9.867a.625.625 0 1 0 0-1.25.625.625 0 0 0 0 1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M21 18v-3a1.5 1.5 0 0 0-1.5-1.5h-15A1.5 1.5 0 0 0 3 15v3a1.5 1.5 0 0 0 1.5 1.5h15A1.5 1.5 0 0 0 21 18Zm1.5 0v-3.884c0-.406-.082-.809-.243-1.182L18.78 4.818A3 3 0 0 0 16.022 3H7.978a3 3 0 0 0-2.757 1.818l-3.478 8.116a3 3 0 0 0-.243 1.182V18a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3ZM17.4 5.41l2.868 6.69a3.004 3.004 0 0 0-.768-.1h-15c-.265 0-.523.034-.768.1L6.6 5.41a1.5 1.5 0 0 1 1.38-.91h8.043a1.5 1.5 0 0 1 1.379.91Zm1.35 11.84a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M28 24v-4a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v4a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2Zm2 0v-5.179a4 4 0 0 0-.323-1.576l-4.638-10.82A4 4 0 0 0 21.362 4H10.638A4 4 0 0 0 6.96 6.424L2.323 17.245A4 4 0 0 0 2 18.821V24a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4ZM23.2 7.212l3.824 8.92A4.003 4.003 0 0 0 26 16H6a4.02 4.02 0 0 0-1.024.132L8.8 7.212A2 2 0 0 1 10.638 6h10.724a2 2 0 0 1 1.839 1.212ZM25 23a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z";
}