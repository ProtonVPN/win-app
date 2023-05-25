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

public class EnvelopeOpen : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M14 8.195V11.8c0 .577 0 .949-.024 1.232-.022.272-.06.373-.085.422a1 1 0 0 1-.437.437c-.05.025-.15.063-.422.085C12.75 14 12.377 14 11.8 14H4.2c-.577 0-.949 0-1.232-.024-.272-.022-.373-.06-.422-.085a1 1 0 0 1-.437-.437c-.025-.05-.063-.15-.085-.422C2 12.75 2 12.377 2 11.8V8.195l5.213 3.218a1.498 1.498 0 0 0 1.574 0L14 8.195Zm-.004-1.173-5.735 3.54a.496.496 0 0 1-.522 0l-5.735-3.54a1.39 1.39 0 0 1 .032-.347 1 1 0 0 1 .157-.324c.062-.084.154-.167.645-.554l3.8-2.995c.365-.288.598-.47.787-.599.178-.12.262-.153.311-.166a1 1 0 0 1 .528 0c.05.013.133.046.311.166.19.128.422.31.787.599l3.8 2.995c.491.387.583.47.645.554a1 1 0 0 1 .156.324c.018.064.028.138.033.347ZM1 7.524c0-.57 0-.854.073-1.117a2 2 0 0 1 .314-.647c.16-.22.385-.396.832-.749l3.8-2.995c.708-.558 1.062-.837 1.453-.944a2 2 0 0 1 1.056 0c.391.107.745.386 1.453.944l3.8 2.995c.447.353.671.53.832.75.143.194.25.413.314.646.073.263.073.548.073 1.117V11.8c0 1.12 0 1.68-.218 2.108a2 2 0 0 1-.874.874C13.48 15 12.92 15 11.8 15H4.2c-1.12 0-1.68 0-2.108-.218a2 2 0 0 1-.874-.874C1 13.48 1 12.92 1 11.8V7.524Z";
}