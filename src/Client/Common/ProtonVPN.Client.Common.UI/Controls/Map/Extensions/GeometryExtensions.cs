/*
 * Copyright (c) 2025 Proton AG
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

using Mapsui;
using NetTopologySuite.Geometries;

namespace ProtonVPN.Client.Common.UI.Controls.Map.Extensions;

public static class GeometryExtensions
{
    public static MRect GetBoundingBox(this Geometry geometry)
    {
        double minX = double.PositiveInfinity;
        double minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        double maxY = double.NegativeInfinity;

        foreach (Coordinate? coord in geometry.Envelope.Coordinates)
        {
            if (coord.X < minX)
            {
                minX = coord.X;
            }
            if (coord.Y < minY)
            {
                minY = coord.Y;
            }
            if (coord.X > maxX)
            {
                maxX = coord.X;
            }
            if (coord.Y > maxY)
            {
                maxY = coord.Y;
            }
        }

        return new MRect(minX, minY, maxX, maxY);
    }
}