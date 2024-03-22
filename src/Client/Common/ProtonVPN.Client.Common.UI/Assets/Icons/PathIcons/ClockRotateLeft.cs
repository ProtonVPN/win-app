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

public class ClockRotateLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3.074 7.596A5.501 5.501 0 0 1 14 8.5a5.5 5.5 0 0 1-9.9 3.3l-.8.6a6.5 6.5 0 1 0-1.205-5.013l-.741-.74a.5.5 0 1 0-.708.707l1.5 1.5a.5.5 0 0 0 .602.08l1.75-1a.5.5 0 1 0-.496-.868l-.928.53Z M9 5.5a.5.5 0 0 0-1 0v3.207l1.646 1.647a.5.5 0 0 0 .708-.708L9 8.293V5.5Z";

    protected override string IconGeometry20 { get; }
        = "M9.952 3.125c-3.503 0-6.394 2.585-6.856 5.933l.646-.641a.63.63 0 0 1 .887 0 .62.62 0 0 1 0 .88L3.028 10.89a.881.881 0 0 1-1.242 0l-1.6-1.591a.62.62 0 0 1 0-.881.63.63 0 0 1 .886 0l.753.748c.42-4.098 3.901-7.29 8.127-7.29 4.508 0 8.17 3.634 8.17 8.125 0 4.49-3.662 8.125-8.17 8.125a8.169 8.169 0 0 1-5.835-2.437l.89-.878a6.919 6.919 0 0 0 4.945 2.065c3.825 0 6.92-3.082 6.92-6.875s-3.095-6.875-6.92-6.875Zm1.443 9.19a.63.63 0 0 0 .886 0 .62.62 0 0 0 0-.88l-1.703-1.693V6.25a.625.625 0 0 0-.626-.623.625.625 0 0 0-.627.623V10a.62.62 0 0 0 .183.44l1.887 1.876Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.942 3.75c-4.204 0-7.672 3.101-8.226 7.12l.774-.77a.755.755 0 0 1 1.064 0 .745.745 0 0 1 0 1.057l-1.921 1.91a1.058 1.058 0 0 1-1.49 0l-1.92-1.91a.745.745 0 0 1 0-1.057.755.755 0 0 1 1.063 0l.904.898C2.694 6.08 6.872 2.25 11.942 2.25c5.41 0 9.803 4.361 9.803 9.75s-4.393 9.75-9.803 9.75a9.803 9.803 0 0 1-7.001-2.925l1.068-1.053a8.303 8.303 0 0 0 5.933 2.478c4.59 0 8.303-3.698 8.303-8.25s-3.713-8.25-8.303-8.25Zm1.731 11.029a.755.755 0 0 0 1.064 0 .745.745 0 0 0 0-1.058l-2.043-2.03V7.5a.75.75 0 0 0-.752-.748.75.75 0 0 0-.752.748V12c0 .198.079.389.22.529l2.263 2.25Z"; 

    protected override string IconGeometry32 { get; }
        = "M15.923 5c-5.605 0-10.23 4.135-10.969 9.493l1.033-1.026a1.007 1.007 0 0 1 1.419 0 .993.993 0 0 1 0 1.41l-2.562 2.546a1.41 1.41 0 0 1-1.986 0L.297 14.877a.993.993 0 0 1 0-1.41 1.007 1.007 0 0 1 1.418 0l1.204 1.197C3.592 8.107 9.162 3 15.923 3c7.213 0 13.071 5.815 13.071 13s-5.858 13-13.071 13a13.07 13.07 0 0 1-9.335-3.9l1.424-1.404A11.07 11.07 0 0 0 15.922 27c6.12 0 11.072-4.93 11.072-11S22.043 5 15.923 5Zm2.308 14.705c.392.39 1.027.39 1.419 0a.993.993 0 0 0 0-1.41l-2.724-2.708V10a1 1 0 0 0-1.003-.997A1 1 0 0 0 14.92 10v6c0 .264.105.518.293.705l3.018 3Z";
}