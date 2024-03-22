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

public class CheckmarkTriple : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M12.103 1.146a.5.5 0 0 1 .002.707L7.74 6.241a.7.7 0 0 1-.995-.002L4.394 3.852a.5.5 0 0 1 .712-.702l2.14 2.171 4.15-4.174a.5.5 0 0 1 .707-.001ZM12.1 9.647a.5.5 0 0 1 .005.708L7.744 14.79a.7.7 0 0 1-1.002-.005L4.39 12.352a.5.5 0 0 1 .72-.695l2.137 2.213 4.147-4.217a.5.5 0 0 1 .707-.006Zm.004-3.293a.5.5 0 0 0-.71-.704L7.246 9.847 5.108 7.654a.5.5 0 1 0-.716.698l2.352 2.412a.7.7 0 0 0 .999.004l4.362-4.414Z";

    protected override string IconGeometry20 { get; }
        = "M15.128 1.432c.245.244.246.64.003.884L9.676 7.802a.875.875 0 0 1-1.243-.003l-2.94-2.984a.625.625 0 0 1 .89-.878l2.674 2.714 5.187-5.217a.625.625 0 0 1 .884-.002Zm-.002 10.627c.246.243.25.638.007.885L9.68 18.488a.875.875 0 0 1-1.252-.005l-2.94-3.042a.626.626 0 0 1 .899-.87l2.672 2.766 5.183-5.27a.625.625 0 0 1 .884-.008Zm.006-4.117a.626.626 0 0 0-.889-.879l-5.185 5.246-2.673-2.742a.625.625 0 1 0-.895.873l2.94 3.015a.875.875 0 0 0 1.248.005l5.454-5.518Z"; 

    protected override string IconGeometry24 { get; }
        = "M18.154 1.718a.75.75 0 0 1 .003 1.061L11.61 9.362a1.05 1.05 0 0 1-1.492-.003L6.59 5.779a.75.75 0 0 1 1.068-1.054l3.21 3.257 6.224-6.26a.75.75 0 0 1 1.06-.004Zm-.003 12.753c.295.29.3.766.009 1.061l-6.543 6.654a1.05 1.05 0 0 1-1.504-.006l-3.527-3.651a.75.75 0 0 1 1.078-1.043l3.207 3.319 6.22-6.325a.75.75 0 0 1 1.06-.009Zm.007-4.94a.75.75 0 0 0-1.066-1.055L10.87 14.77l-3.208-3.29a.75.75 0 1 0-1.074 1.048l3.528 3.618a1.05 1.05 0 0 0 1.498.005l6.544-6.62Z"; 

    protected override string IconGeometry32 { get; }
        = "M24.205 2.291a1 1 0 0 1 .004 1.415l-8.727 8.777a1.4 1.4 0 0 1-1.99-.005L8.788 7.706A1 1 0 0 1 10.212 6.3l4.28 4.342 8.299-8.347a1 1 0 0 1 1.414-.004Zm-.004 17.004a1 1 0 0 1 .012 1.415l-8.724 8.872a1.4 1.4 0 0 1-2.005-.01l-4.703-4.867a1 1 0 0 1 1.438-1.39l4.275 4.425 8.293-8.434a1 1 0 0 1 1.414-.011Zm.01-6.587a1 1 0 0 0-1.422-1.407l-8.296 8.393-4.277-4.387a1 1 0 1 0-1.432 1.398l4.704 4.823a1.4 1.4 0 0 0 1.998.007l8.725-8.827Z";
}