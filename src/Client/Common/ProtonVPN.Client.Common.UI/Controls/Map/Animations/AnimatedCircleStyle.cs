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

using Mapsui.Styles;

namespace ProtonVPN.Client.Common.UI.Controls.Map.Animations;

public class AnimatedCircleStyle : IStyle
{
    public const float TRANSPARENT_CIRCLE_RADIUS_DEFAULT = 8f;
    public const float TRANSPARENT_CIRCLE_RADIUS_HOVER = 16f;
    public const float TRANSPARENT_CIRCLE_RADIUS_CONNECTING = 0f;
    public const float TRANSPARENT_CIRCLE_RADIUS_ACTIVE = 48f;

    public const float NEUTRAL_CIRCLE_RADIUS_DEFAULT = 3f;
    public const float NEUTRAL_CIRCLE_RADIUS_HOVER = 8f;
    public const float NEUTRAL_CIRCLE_RADIUS_CONNECTING = 6f;
    public const float NEUTRAL_CIRCLE_RADIUS_ACTIVE = 12f;

    public const float CENTER_CIRCLE_RADIUS_DEFAULT = 0f;
    public const float CENTER_CIRCLE_RADIUS_HOVER = 4f;
    public const float CENTER_CIRCLE_RADIUS_CONNECTING = 3f;
    public const float CENTER_CIRCLE_RADIUS_ACTIVE = 6f;

    public float TransparentCircleRadius { get; set; } = TRANSPARENT_CIRCLE_RADIUS_DEFAULT;
    public float NeutralCircleRadius { get; set; } = NEUTRAL_CIRCLE_RADIUS_DEFAULT;
    public float CenterCircleRadius { get; set; } = CENTER_CIRCLE_RADIUS_DEFAULT;

    public double MinVisible { get; set; } = double.MinValue;
    public double MaxVisible { get; set; } = double.MaxValue;

    public bool Enabled { get; set; } = true;
    public float Opacity { get; set; } = 1f;
}