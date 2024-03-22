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

public class ArrowRotateRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.146 1.647a.5.5 0 0 1 .708 0l1.358 1.358a.7.7 0 0 1 0 .99L7.854 5.354a.5.5 0 0 1-.708-.707l.642-.642a5 5 0 1 0 3.52 1.245.5.5 0 0 1 .66-.75 6 6 0 1 1-4.173-1.497l-.649-.65a.5.5 0 0 1 0-.706Z";

    protected override string IconGeometry20 { get; }
        = "M8.933 1.433a.625.625 0 0 1 .884 0l1.698 1.698a.875.875 0 0 1 0 1.238L9.817 6.067a.625.625 0 0 1-.884-.884l.803-.802a6.25 6.25 0 1 0 4.398 1.557A.625.625 0 1 1 14.96 5a7.5 7.5 0 1 1-5.216-1.87l-.812-.813a.625.625 0 0 1 0-.884Zm1.698 2.582Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.72 1.72a.75.75 0 0 1 1.06 0l2.038 2.038c.41.41.41 1.075 0 1.485L11.78 7.28a.75.75 0 1 1-1.06-1.06l.963-.963a7.5 7.5 0 1 0 5.278 1.868A.75.75 0 0 1 17.953 6a9 9 0 1 1-6.259-2.245l-.974-.975a.75.75 0 0 1 0-1.06Zm2.037 3.098Z"; 

    protected override string IconGeometry32 { get; }
        = "M14.293 2.293a1 1 0 0 1 1.414 0l2.717 2.717a1.4 1.4 0 0 1 0 1.98l-2.717 2.717a1 1 0 1 1-1.414-1.414l1.284-1.284C10.25 7.231 6 11.62 6 17c0 5.523 4.477 10 10 10s10-4.477 10-10a9.973 9.973 0 0 0-3.386-7.5A1 1 0 1 1 23.938 8 11.973 11.973 0 0 1 28 17c0 6.628-5.373 12-12 12S4 23.628 4 17C4 10.51 9.154 5.222 15.593 5.007l-1.3-1.3a1 1 0 0 1 0-1.414Zm2.716 4.131h.001Z";
}