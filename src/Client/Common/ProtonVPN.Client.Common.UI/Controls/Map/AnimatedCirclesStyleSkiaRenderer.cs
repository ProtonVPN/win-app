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
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using Microsoft.UI.Xaml;
using NetTopologySuite.Geometries;
using ProtonVPN.Client.Common.UI.Controls.Map.Animations;
using SkiaSharp;

namespace ProtonVPN.Client.Common.UI.Controls.Map;

public class AnimatedCirclesStyleSkiaRenderer : ISkiaStyleRenderer
{
    public ElementTheme ElementTheme { get; set; }

    private string GreenColor => ElementTheme == ElementTheme.Dark
        ? "#2CFFCC"
        : "#1C9C7C";

    private string RedColor => ElementTheme == ElementTheme.Dark
        ? "#F7607B"
        : "#B71432";

    public bool Draw(
        SKCanvas canvas,
        Viewport viewport,
        ILayer layer,
        IFeature feature,
        IStyle style,
        IRenderCache renderCache,
        long iteration)
    {
        if (style is not AnimatedCircleStyle animatedCircleStyle)
        {
            return false;
        }

        if (feature is not GeometryFeature geometryFeature)
        {
            return false;
        }

        if (geometryFeature.Geometry is not Point point)
        {
            return false;
        }

        MPoint screenPos = viewport.WorldToScreen(point.ToMPoint());

        double layerOpacity = feature.IsCurrentCountry() || feature.IsConnecting()
            ? 1
            : layer.Opacity;

        DrawTransparentCircle(canvas, animatedCircleStyle, screenPos, feature, layerOpacity);
        DrawNeutralCircle(canvas, animatedCircleStyle, screenPos, feature, layerOpacity);
        DrawCenterCircle(canvas, animatedCircleStyle, screenPos, feature, layerOpacity);

        return true;
    }

    private void DrawTransparentCircle(
        SKCanvas canvas,
        AnimatedCircleStyle style,
        MPoint screenPos,
        IFeature feature,
        double layerOpacity)
    {
        if (feature.IsCurrentCountry() && !feature.IsOnHover())
        {
            SKColor baseColor = SKColor.Parse(feature.IsConnected()
                ? GreenColor
                : RedColor);

            SKColor centerColor = baseColor.WithAlpha(0);
            SKColor edgeColor = baseColor.WithAlpha(255);

            const float gradientOpacity = 0.5f;
            centerColor = centerColor.WithAlpha((byte)(centerColor.Alpha * gradientOpacity));
            edgeColor = edgeColor.WithAlpha((byte)(edgeColor.Alpha * gradientOpacity));

            using SKPaint outerPaint = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint((float)screenPos.X, (float)screenPos.Y),
                    style.TransparentCircleRadius,
                    [centerColor, edgeColor],
                    [0.442708f, 1],
                    SKShaderTileMode.Clamp
                )
            };

            canvas.DrawCircle(
                (float)screenPos.X,
                (float)screenPos.Y,
                style.TransparentCircleRadius,
                outerPaint);
        }
        else
        {
            SKColor color = ElementTheme == ElementTheme.Dark
                ? SKColors.White.WithAlpha(40)
                : SKColor.Parse("#BCBCC3").WithAlpha(40);

            using SKPaint outerPaint = new()
            {
                IsAntialias = true,
                Color = color.WithAlpha(
                    (byte)(color.Alpha * layerOpacity))
            };

            canvas.DrawCircle(
                (float)screenPos.X,
                (float)screenPos.Y,
                style.TransparentCircleRadius,
                outerPaint);
        }
    }

    private void DrawNeutralCircle(
        SKCanvas canvas,
        AnimatedCircleStyle style,
        MPoint screenPos,
        IFeature feature,
        double layerOpacity)
    {
        SKColor color = ElementTheme == ElementTheme.Dark
            ? SKColors.White
            : feature.IsCurrentCountry()
                ? SKColors.White
                : SKColor.Parse("#BCBCC3");

        using SKPaint middlePaint = new()
        {
            IsAntialias = true,
            Color = GetNeutralCircleColor(feature, layerOpacity)
        };
        canvas.DrawCircle((float)screenPos.X, (float)screenPos.Y, style.NeutralCircleRadius, middlePaint);
    }

    private void DrawCenterCircle(
        SKCanvas canvas,
        AnimatedCircleStyle style,
        MPoint screenPos,
        IFeature feature,
        double layerOpacity)
    {
        if (style.CenterCircleRadius > 0)
        {
            using SKPaint innerPaint = new()
            {
                IsAntialias = true,
                Color = GetCenterCircleColor(feature, layerOpacity),
            };
            canvas.DrawCircle((float)screenPos.X, (float)screenPos.Y, style.CenterCircleRadius, innerPaint);
        }
    }

    private SKColor GetNeutralCircleColor(IFeature feature, double layerOpacity)
    {
        bool isHighlighted = feature.IsOnHover() || feature.IsCurrentCountry() || ElementTheme == ElementTheme.Dark;
        SKColor baseColor = isHighlighted ? SKColors.White : SKColor.Parse("#898990");
        return baseColor.WithAlpha((byte)(layerOpacity * 255));
    }

    private SKColor GetCenterCircleColor(IFeature feature, double layerOpacity)
    {
        SKColor color = SKColors.White;

        if (feature.IsOnHover())
        {
            color = SKColor.Parse("#8A6EFF");
        }
        else if (feature.IsConnected())
        {
            color = SKColor.Parse(GreenColor);
        }
        else if (feature.IsConnecting())
        {
            color = SKColor.Parse("#6E6B79");
        }
        else if (feature.IsCurrentCountry())
        {
            color = SKColor.Parse(RedColor);
        }

        return color.WithAlpha((byte)(layerOpacity * 255));
    }
}