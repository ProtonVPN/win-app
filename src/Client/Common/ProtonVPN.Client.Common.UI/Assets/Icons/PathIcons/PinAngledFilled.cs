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

public class PinAngledFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M9.704 1.473a.61.61 0 0 0-1.042.432v1.133c0 .509-.316.964-.793 1.143L2.644 6.14a.61.61 0 0 0-.218 1.003l2.75 2.75-3.422 3.421a.61.61 0 1 0 .863.864l3.422-3.422 2.816 2.815a.61.61 0 0 0 1.003-.217l1.96-5.226a1.22 1.22 0 0 1 1.142-.792h1.134a.61.61 0 0 0 .431-1.042L9.704 1.473Z";

    protected override string IconGeometry20 { get; }
        = "M12.13 1.841a.763.763 0 0 0-1.303.54v1.416c0 .636-.395 1.206-.99 1.43L3.304 7.675a.763.763 0 0 0-.271 1.254l3.437 3.437-4.278 4.277a.763.763 0 1 0 1.08 1.08l4.277-4.278 3.52 3.519a.763.763 0 0 0 1.253-.272l2.45-6.532c.223-.596.792-.99 1.429-.99h1.416c.68 0 1.02-.822.54-1.303L12.13 1.841Z"; 

    protected override string IconGeometry24 { get; }
        = "M14.555 2.21c-.576-.577-1.563-.169-1.563.647v1.7a1.83 1.83 0 0 1-1.188 1.714l-7.839 2.94a.916.916 0 0 0-.326 1.505l4.125 4.124-5.133 5.133a.916.916 0 1 0 1.295 1.295l5.133-5.133 4.223 4.223a.916.916 0 0 0 1.505-.326l2.94-7.839a1.831 1.831 0 0 1 1.714-1.188h1.7c.815 0 1.224-.986.647-1.563l-7.233-7.233Z"; 

    protected override string IconGeometry32 { get; }
        = "M19.407 2.946c-.769-.77-2.084-.224-2.084.863v2.267a2.442 2.442 0 0 1-1.584 2.286l-10.452 3.92a1.22 1.22 0 0 0-.435 2.005l5.5 5.5-6.844 6.844a1.22 1.22 0 1 0 1.726 1.726l6.844-6.844 5.631 5.631a1.22 1.22 0 0 0 2.006-.434l3.92-10.453a2.441 2.441 0 0 1 2.286-1.584h2.266c1.088 0 1.633-1.315.863-2.084l-9.643-9.643Z";
}