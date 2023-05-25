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

public class Bug : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M10.443 3.19c.16.226.29.475.385.742H12.5v2.015c.825 0 1.5-.674 1.5-1.512a.5.5 0 0 1 1 0 2.506 2.506 0 0 1-2.5 2.512v1.516h2a.5.5 0 0 1 0 1h-2V10.5c0 .337-.037.665-.107.981h.107c1.384 0 2.5 1.128 2.5 2.512a.5.5 0 0 1-1 0c0-.838-.675-1.512-1.5-1.512h-.459a4.5 4.5 0 0 1-8.082 0H3.5c-.826 0-1.5.674-1.5 1.512a.5.5 0 0 1-1 0 2.506 2.506 0 0 1 2.5-2.512h.107A4.515 4.515 0 0 1 3.5 10.5V9.463h-2a.5.5 0 0 1 0-1h2V6.947A2.506 2.506 0 0 1 1 4.435a.5.5 0 1 1 1 0c0 .838.674 1.512 1.5 1.512V3.932h1.672c.094-.267.225-.516.385-.743L4.233 1.857a.504.504 0 0 1 0-.709.496.496 0 0 1 .705 0L6.262 2.48c.49-.353 1.09-.56 1.738-.56.648 0 1.248.207 1.738.56l1.324-1.332a.496.496 0 0 1 .705 0 .504.504 0 0 1 0 .71l-1.324 1.331ZM6 4.937H5v-.006h-.5V10.5a3.5 3.5 0 1 0 7 0V4.932H11v.006H6Zm3.732-1.006A2.003 2.003 0 0 0 8 2.919c-.736 0-1.385.408-1.732 1.013h3.464Z";
}