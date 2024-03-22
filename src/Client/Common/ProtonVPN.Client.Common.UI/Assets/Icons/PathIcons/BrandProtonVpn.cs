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

public class BrandProtonVpn : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.796 1.512C1.496 1.356.57 2.74 1.209 3.882l5.512 9.85a1.5 1.5 0 0 0 2.573.074l5.457-8.551a1.6 1.6 0 0 0-1.159-2.45L2.796 1.513Zm-.714 1.881a.6.6 0 0 1 .595-.888l10.796 1.293a.6.6 0 0 1 .435.919l-5.457 8.55a.5.5 0 0 1-.858-.024l-.32-.571 4.23-6.466a.758.758 0 0 0-.533-1.166L2.355 3.882l-.273-.489Zm.934 1.67 3.66 6.542 3.639-5.56-7.299-.981Z";

    protected override string IconGeometry20 { get; }
        = "M3.492 1.885C1.868 1.691.71 3.42 1.509 4.847l6.888 12.31c.693 1.237 2.454 1.288 3.217.093l6.82-10.687a2 2 0 0 0-1.448-3.06L3.492 1.884Zm-.893 2.352a.75.75 0 0 1 .744-1.111l13.494 1.617a.75.75 0 0 1 .543 1.148l-6.82 10.687a.625.625 0 0 1-1.072-.031l-.4-.714 5.287-8.08a.948.948 0 0 0-.667-1.459L2.941 4.847l-.342-.61Zm1.169 2.088L8.343 14.5l4.546-6.95-9.121-1.225Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.19 2.268C2.243 2.035.853 4.111 1.81 5.823L10.08 20.6c.83 1.484 2.945 1.546 3.86.111l8.186-12.827c.951-1.49.018-3.464-1.738-3.674L4.191 2.269ZM3.12 5.091a.9.9 0 0 1 .892-1.333l16.197 1.94a.9.9 0 0 1 .651 1.378l-8.185 12.827a.75.75 0 0 1-1.287-.037l-.48-.857 6.346-9.699c.458-.7.028-1.638-.8-1.75L3.53 5.824l-.41-.733Zm1.402 2.506 5.49 9.812 5.458-8.341L4.522 7.597Z"; 

    protected override string IconGeometry32 { get; }
        = "M5.588 3.024c-2.598-.311-4.452 2.457-3.174 4.74l11.024 19.7c1.108 1.98 3.927 2.062 5.147.15L29.5 10.51c1.268-1.988.024-4.618-2.317-4.9L5.588 3.025ZM4.16 6.788A1.2 1.2 0 0 1 5.35 5.01l21.595 2.588a1.2 1.2 0 0 1 .869 1.837L16.899 26.538a1 1 0 0 1-1.715-.05l-.64-1.142 8.46-12.932a1.517 1.517 0 0 0-1.067-2.334L4.707 7.765l-.547-.977Zm1.87 3.34 7.32 13.084 7.277-11.122-14.598-1.96Z";
}