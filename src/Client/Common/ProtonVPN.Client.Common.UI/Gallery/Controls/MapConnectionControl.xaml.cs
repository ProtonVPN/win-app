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

using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;
using Windows.UI;

namespace ProtonVPN.Client.Common.UI.Gallery.Controls;

public sealed partial class MapConnectionControl : UserControl
{
    public static readonly DependencyProperty BrushEndPointProperty =
        DependencyProperty.Register(nameof(BrushEndPoint), typeof(Point), typeof(MapConnectionControl), new PropertyMetadata(null));

    public static readonly DependencyProperty BrushStartPointProperty =
        DependencyProperty.Register(nameof(BrushStartPoint), typeof(Point), typeof(MapConnectionControl), new PropertyMetadata(null));

    public static readonly DependencyProperty ConnectionBrushOffsetProperty =
        DependencyProperty.Register(nameof(ConnectionBrushOffset), typeof(double), typeof(MapConnectionControl), new PropertyMetadata(0.0));

    public static readonly DependencyProperty Offset1ColorProperty =
        DependencyProperty.Register(nameof(Offset1Color), typeof(Color), typeof(MapConnectionControl), new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty Offset2ColorProperty =
        DependencyProperty.Register(nameof(Offset2Color), typeof(Color), typeof(MapConnectionControl), new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty Offset3ColorProperty =
        DependencyProperty.Register(nameof(Offset3Color), typeof(Color), typeof(MapConnectionControl), new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty Offset4ColorProperty =
        DependencyProperty.Register(nameof(Offset4Color), typeof(Color), typeof(MapConnectionControl), new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty SourcePinProperty =
        DependencyProperty.Register(nameof(SourcePin), typeof(MapPinControl), typeof(MapConnectionControl), new PropertyMetadata(null, OnPinPropertyChanged));

    public static readonly DependencyProperty TargetPinProperty =
        DependencyProperty.Register(nameof(TargetPin), typeof(MapPinControl), typeof(MapConnectionControl), new PropertyMetadata(null, OnPinPropertyChanged));

    public MapConnectionControl()
    {
        InitializeComponent();
    }

    public Point BrushEndPoint
    {
        get => (Point)GetValue(BrushEndPointProperty);
        set => SetValue(BrushEndPointProperty, value);
    }

    public Point BrushStartPoint
    {
        get => (Point)GetValue(BrushStartPointProperty);
        set => SetValue(BrushStartPointProperty, value);
    }

    public double ConnectionBrushOffset
    {
        get => (double)GetValue(ConnectionBrushOffsetProperty);
        set => SetValue(ConnectionBrushOffsetProperty, value);
    }

    public Color Offset1Color
    {
        get => (Color)GetValue(Offset1ColorProperty);
        set => SetValue(Offset1ColorProperty, value);
    }

    public Color Offset2Color
    {
        get => (Color)GetValue(Offset2ColorProperty);
        set => SetValue(Offset2ColorProperty, value);
    }

    public Color Offset3Color
    {
        get => (Color)GetValue(Offset3ColorProperty);
        set => SetValue(Offset3ColorProperty, value);
    }

    public Color Offset4Color
    {
        get => (Color)GetValue(Offset4ColorProperty);
        set => SetValue(Offset4ColorProperty, value);
    }

    public MapPinControl SourcePin
    {
        get => (MapPinControl)GetValue(SourcePinProperty);
        set => SetValue(SourcePinProperty, value);
    }

    public MapPinControl TargetPin
    {
        get => (MapPinControl)GetValue(TargetPinProperty);
        set => SetValue(TargetPinProperty, value);
    }

    private static void OnPinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MapConnectionControl connection)
        {
            if (connection.SourcePin is null || connection.TargetPin is null)
            {
                return;
            }

            connection.AnimateGradientBrush(connection.SourcePin.IsTarget || connection.TargetPin.IsTarget);
        }
    }

    private void AnimateGradientBrush(bool isTargetLine)
    {
        Offset1Color = (Color)Application.Current.Resources["GradientConnectionLineStartColor"];
        Offset2Color = isTargetLine
            ? (Color)Application.Current.Resources["GradientConnectionLineEndColor"]
            : (Color)Application.Current.Resources["GradientConnectionLineStartColor"];
        Offset3Color = isTargetLine
            ? Colors.Transparent
            : (Color)Application.Current.Resources["GradientConnectionLineStartColor"];
        Offset4Color = isTargetLine
            ? Colors.Transparent
            : (Color)Application.Current.Resources["GradientConnectionLineEndColor"];

        CalculateBrushStartEndPoint();

        DoubleAnimation animation = new()
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new SineEase()
        };
        Storyboard.SetTarget(animation, this);
        Storyboard.SetTargetProperty(animation, nameof(MapConnectionControl.Opacity));

        Storyboard storyBoard = new();
        storyBoard.Children.Add(animation);
        storyBoard.Begin();
    }

    private void CalculateBrushStartEndPoint()
    {
        //Point end = new Point(SourcePin.Position.X + TargetPin.Position.X, SourcePin.Position.Y + TargetPin.Position.Y);
        //BrushStartPoint = new Point(SourcePin.Position.X / end.X, SourcePin.Position.Y / end.Y);
        //BrushEndPoint = new Point(TargetPin.Position.X / end.X, TargetPin.Position.Y / end.Y);

        BrushStartPoint = new Point(
            SourcePin.Position.X <= TargetPin.Position.X ? 0 : 1,
            SourcePin.Position.Y <= TargetPin.Position.Y ? 0 : 1);
        BrushEndPoint = new Point(
            SourcePin.Position.X <= TargetPin.Position.X ? 1 : 0,
            SourcePin.Position.Y <= TargetPin.Position.Y ? 1 : 0);
    }
}