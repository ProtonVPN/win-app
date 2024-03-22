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

public class ArrowsCross : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M10.294 4.418 9 3.124V8h4.876l-1.294-1.293a.5.5 0 0 1 .707-.708l2.006 2.006a.7.7 0 0 1 0 .99l-2.006 2.006a.5.5 0 0 1-.707-.707L13.875 9H9v4.876l1.294-1.294a.5.5 0 0 1 .707.707l-2.006 2.006a.7.7 0 0 1-.99 0L5.999 13.29a.5.5 0 1 1 .707-.708L8 13.876V9H3.124l1.294 1.294a.5.5 0 1 1-.707.707L1.705 8.995a.7.7 0 0 1 0-.99L3.711 6a.5.5 0 0 1 .707.708L3.124 8H8V3.124L6.706 4.418A.5.5 0 0 1 6 3.71l2.006-2.006a.7.7 0 0 1 .99 0L11 3.711a.5.5 0 0 1-.707.707Z";

    protected override string IconGeometry20 { get; }
        = "M12.247 4.902 10.63 3.285V9.38h6.094l-1.617-1.617a.625.625 0 1 1 .884-.884l2.507 2.507a.875.875 0 0 1 0 1.238l-2.507 2.507a.625.625 0 1 1-.884-.884l1.617-1.617H10.63v6.095l1.617-1.617a.625.625 0 0 1 .884.883L10.623 18.5a.875.875 0 0 1-1.237 0l-2.507-2.507a.625.625 0 1 1 .883-.884l1.618 1.617V10.63H3.285l1.617 1.617a.625.625 0 0 1-.884.884l-2.507-2.508a.875.875 0 0 1 0-1.237L4.018 6.88a.625.625 0 0 1 .884.884L3.285 9.38H9.38V3.285L7.762 4.902a.625.625 0 0 1-.883-.884l2.507-2.507a.875.875 0 0 1 1.237 0l2.508 2.507a.625.625 0 0 1-.884.884Z"; 

    protected override string IconGeometry24 { get; }
        = "m14.696 5.882-1.94-1.94v7.314h7.313l-1.94-1.94a.75.75 0 1 1 1.06-1.061l3.009 3.008c.41.41.41 1.075 0 1.485l-3.009 3.009a.75.75 0 0 1-1.06-1.06l1.94-1.941h-7.313v7.314l1.94-1.94a.75.75 0 0 1 1.06 1.06l-3.008 3.008a1.05 1.05 0 0 1-1.485 0L8.254 19.19a.75.75 0 1 1 1.061-1.06l1.94 1.94v-7.314H3.942l1.94 1.94a.75.75 0 0 1-1.06 1.06l-3.009-3.008a1.05 1.05 0 0 1 0-1.485l3.009-3.008a.75.75 0 0 1 1.06 1.06l-1.94 1.94h7.313V3.943l-1.94 1.94a.75.75 0 0 1-1.06-1.06l3.008-3.009a1.05 1.05 0 0 1 1.485 0l3.009 3.009a.75.75 0 1 1-1.061 1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "m19.595 7.843-2.588-2.587v9.752h9.752L24.17 12.42a1 1 0 0 1 1.414-1.415l4.012 4.012a1.4 1.4 0 0 1 0 1.98l-4.012 4.011a1 1 0 1 1-1.414-1.414l2.588-2.587h-9.752v9.752l2.588-2.588a1 1 0 0 1 1.414 1.415l-4.012 4.011a1.4 1.4 0 0 1-1.98 0l-4.011-4.011a1 1 0 1 1 1.414-1.415l2.587 2.588v-9.752h-9.75l2.586 2.587a1 1 0 1 1-1.414 1.414l-4.011-4.011a1.4 1.4 0 0 1 0-1.98l4.011-4.012a1 1 0 1 1 1.414 1.415l-2.587 2.587h9.751V5.256L12.42 7.843a1 1 0 0 1-1.414-1.414l4.011-4.012a1.4 1.4 0 0 1 1.98 0L21.01 6.43a1 1 0 0 1-1.414 1.414Z";
}