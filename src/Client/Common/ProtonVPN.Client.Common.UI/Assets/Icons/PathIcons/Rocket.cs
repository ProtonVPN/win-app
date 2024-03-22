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

public class Rocket : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7 9a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0Zm1.5-.5a.5.5 0 1 0 0 1 .5.5 0 0 0 0-1Z M9.07 1.29a.957.957 0 0 0-1.14 0C5.12 3.378 5.004 6.776 5 9.55A2.5 2.5 0 0 0 3 12v2h11v-2a2.5 2.5 0 0 0-2-2.45c-.005-2.775-.121-6.173-2.93-8.26ZM4 12a1.5 1.5 0 0 1 1-1.415V13H4v-1Zm2 1v-3c0-1.35.007-2.726.25-4h4.5c.243 1.274.25 2.65.25 4v3H6Zm4.5-8h-4c.356-1.112.963-2.108 2-2.888 1.037.78 1.644 1.776 2 2.888Zm2.5 8h-1v-2.415A1.5 1.5 0 0 1 13 12v1Z M5.5 15a.5.5 0 0 0 0 1h6a.5.5 0 0 0 0-1h-6Z";

    protected override string IconGeometry20 { get; }
        = "M8.75 11.25a1.875 1.875 0 1 1 3.75 0 1.875 1.875 0 0 1-3.75 0Zm1.875-.625a.625.625 0 1 0 0 1.25.625.625 0 0 0 0-1.25Z M11.338 1.613a1.196 1.196 0 0 0-1.426 0C6.402 4.221 6.256 8.47 6.25 11.938A3.126 3.126 0 0 0 3.75 15v2.5H17.5V15a3.126 3.126 0 0 0-2.5-3.063c-.006-3.468-.152-7.716-3.662-10.324ZM5 15c0-.816.522-1.51 1.25-1.768v3.018H5V15Zm2.5 1.25V12.5c0-1.688.008-3.408.312-5h5.626c.304 1.592.312 3.312.312 5v3.75H7.5Zm5.624-10H8.126c.443-1.39 1.202-2.635 2.499-3.61 1.297.975 2.056 2.22 2.5 3.61Zm3.126 10H15v-3.018c.728.257 1.25.952 1.25 1.768v1.25Z M6.875 18.75a.625.625 0 1 0 0 1.25h7.5a.625.625 0 1 0 0-1.25h-7.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.5 13.5a2.25 2.25 0 1 1 4.5 0 2.25 2.25 0 0 1-4.5 0Zm2.25-.75a.75.75 0 1 0 0 1.5.75.75 0 0 0 0-1.5Z M13.606 1.936a1.435 1.435 0 0 0-1.712 0C7.682 5.065 7.507 10.163 7.5 14.325A3.75 3.75 0 0 0 4.5 18v3H21v-3a3.751 3.751 0 0 0-3-3.675c-.007-4.162-.182-9.26-4.394-12.39ZM6 18c0-.98.626-1.813 1.5-2.122V19.5H6V18Zm3 1.5V15c0-2.025.01-4.09.374-6h6.752c.364 1.91.374 3.975.374 6v4.5H9Zm6.75-12h-6c.533-1.668 1.444-3.162 3-4.332 1.556 1.17 2.467 2.664 3 4.332Zm3.75 12H18v-3.622A2.251 2.251 0 0 1 19.5 18v1.5Z M8.25 22.5a.75.75 0 0 0 0 1.5h9a.75.75 0 0 0 0-1.5h-9Z"; 

    protected override string IconGeometry32 { get; }
        = "M14 18a3 3 0 1 1 6 0 3 3 0 0 1-6 0Zm3-1a1 1 0 1 0 0 2 1 1 0 0 0 0-2Z M18.142 2.581a1.913 1.913 0 0 0-2.284 0C10.243 6.753 10.01 13.551 10 19.1A5.002 5.002 0 0 0 6 24v4h22v-4a5.002 5.002 0 0 0-4-4.9c-.01-5.549-.243-12.347-5.858-16.519ZM8 24c0-1.306.835-2.418 2-2.83V26H8v-2Zm4 2v-6c0-2.7.013-5.453.5-8h9c.487 2.547.5 5.3.5 8v6H12Zm9-16h-8c.711-2.224 1.925-4.216 4-5.775 2.075 1.559 3.289 3.55 4 5.775Zm5 16h-2v-4.83c1.165.412 2 1.524 2 2.83v2Z M11 30a1 1 0 1 0 0 2h12a1 1 0 1 0 0-2H11Z";
}