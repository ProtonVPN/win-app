/*
 * Copyright (c) 2024 Proton AG
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
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace ProtonVPN.Client.Common.UI.Controls.Map;

public sealed partial class MapControl
{
    private const int MAX_SCALE_FACTOR = 15;
    private const string COUNTRY_CODE_PREFIX = "country_";

    public static readonly DependencyProperty ActiveCountryCodeProperty = DependencyProperty.Register(
        nameof(ActiveCountryCode),
        typeof(string),
        typeof(MapControl),
        new PropertyMetadata(null, OnActiveCountryCodeChanged));

    public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register(
        nameof(IsConnected),
        typeof(bool),
        typeof(MapControl),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsConnectingProperty = DependencyProperty.Register(
        nameof(IsConnecting),
        typeof(bool),
        typeof(MapControl),
        new PropertyMetadata(false));

    public static readonly DependencyProperty InactiveFillColorBrushProperty = DependencyProperty.Register(
        nameof(InactiveFillColorBrush),
        typeof(Brush),
        typeof(MapControl),
        new PropertyMetadata(default, OnInactiveFillColorBrushChanged));

    public static readonly DependencyProperty ActiveFillColorBrushProperty = DependencyProperty.Register(
        nameof(ActiveFillColorBrush),
        typeof(Brush),
        typeof(MapControl),
        new PropertyMetadata(default, OnActiveFillColorBrushChanged));

    public static readonly DependencyProperty IsCountryPinVisibleProperty = DependencyProperty.Register(
        nameof(IsCountryPinVisible),
        typeof(bool),
        typeof(MapControl),
        new PropertyMetadata(true));

    public static readonly DependencyProperty IsMainWindowVisibleProperty = DependencyProperty.Register(
        nameof(IsMainWindowVisible),
        typeof(bool),
        typeof(CountryPin),
        new PropertyMetadata(false));

    private Path? _currentCountryPath;

    public bool IsCountryPinVisible
    {
        get => (bool)GetValue(IsCountryPinVisibleProperty);
        set => SetValue(IsCountryPinVisibleProperty, value);
    }

    public string ActiveCountryCode
    {
        get => (string)GetValue(ActiveCountryCodeProperty);
        set => SetValue(ActiveCountryCodeProperty, value);
    }

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public bool IsConnecting
    {
        get => (bool)GetValue(IsConnectingProperty);
        set => SetValue(IsConnectingProperty, value);
    }

    public Brush InactiveFillColorBrush
    {
        get => (Brush)GetValue(InactiveFillColorBrushProperty);
        set => SetValue(InactiveFillColorBrushProperty, value);
    }

    public Brush ActiveFillColorBrush
    {
        get => (Brush)GetValue(ActiveFillColorBrushProperty);
        set => SetValue(ActiveFillColorBrushProperty, value);
    }

    public bool IsMainWindowVisible
    {
        get => (bool)GetValue(IsMainWindowVisibleProperty);
        set => SetValue(IsMainWindowVisibleProperty, value);
    }

    public MapControl()
    {
        InitializeComponent();

        Viewbox.SizeChanged += OnViewboxSizeChanged;
    }

    public void InvalidateDpi()
    {
        // When DPI changes, remove and reset canvas to avoid map being cropped
        Viewbox.Child = null;
        Viewbox.Child = MapCanvas;
    }

    private static void OnActiveCountryCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        if (e.OldValue is string oldCountryCode)
        {
            control.MakeCountryInactive(oldCountryCode);
        }

        if (e.NewValue is string newCountryCode)
        {
            control.MakeCountryActive(newCountryCode);
        }

        control.Pin.Opacity = string.IsNullOrEmpty(control.ActiveCountryCode) ? 0 : 1;
    }

    private static void OnInactiveFillColorBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        control.InvalidateCountryFill();
    }

    private static void OnActiveFillColorBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        control.InvalidateCountryFill();
    }

    private void OnViewboxSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_currentCountryPath is null)
        {
            return;
        }

        ZoomAndPanToCountryCenter(_currentCountryPath);
    }

    private void ZoomAndPanToCountryCenter(Path countryPath)
    {
        double scale = GetScaleFactor(countryPath);

        ScaleMap(scale);

        string countryCode = ActiveCountryCode;
        if (_coordinates.ContainsKey(countryCode))
        {
            TranslateToPin(countryCode, scale);
        }
        else
        {
            TranslateToCountryCenter(countryPath, scale);
        }
    }

    private double GetScaleFactor(Path countryPath)
    {
        double divisor = GetCountryViewBoxDivisor(countryPath);
        double maxCountryWidth = Viewbox.ActualWidth / divisor;
        double maxCountryHeight = Viewbox.ActualHeight / divisor;

        double width = Viewbox.ActualWidth / MapCanvas.Width * countryPath.Data.Bounds.Width;
        double height = Viewbox.ActualHeight / MapCanvas.Height * countryPath.Data.Bounds.Height;

        double scaleFactor = height > width ? maxCountryHeight / height : maxCountryWidth / width;

        return Math.Min(scaleFactor, MAX_SCALE_FACTOR);
    }

    private double GetCountryViewBoxDivisor(Path countryPath)
    {
        double area = countryPath.Data.Bounds.Width * countryPath.Data.Bounds.Height;
        return area switch
        {
            >= 10000 => 1.5,
            >= 5000 => 2,
            >= 1000 => 2.5,
            >= 500 => 3,
            _ => 4
        };
    }

    private void ScaleMap(double scale)
    {
        MapCanvasScaleTransform.ScaleX = scale;
        MapCanvasScaleTransform.ScaleY = scale;
    }

    private void TranslateMapToCoordinate(double x, double y, double scale)
    {
        double canvasCenterX = MapCanvas.Width / 2;
        double canvasCenterY = MapCanvas.Height / 2;

        MapCanvasTranslateTransform.X = canvasCenterX - (x * scale);
        MapCanvasTranslateTransform.Y = canvasCenterY - (y * scale);
    }

    private void TranslateToPin(string countryCode, double scale)
    {
        double x = _coordinates[countryCode].X;
        double y = _coordinates[countryCode].Y;

        TranslateMapToCoordinate(x, y, scale);
    }

    private void TranslateToCountryCenter(Path country, double scale)
    {
        double x = country.Data.Bounds.Left + (country.Data.Bounds.Width / 2);
        double y = country.Data.Bounds.Top + (country.Data.Bounds.Height / 2);

        TranslateMapToCoordinate(x, y, scale);
    }

    private void MakeCountryInactive(string countryCode)
    {
        Path? countryPath = GetCountryPath(countryCode);
        if (countryPath is not null)
        {
            countryPath.Fill = InactiveFillColorBrush;
        }
    }

    private void MakeCountryActive(string countryCode)
    {
        Path? countryPath = GetCountryPath(countryCode);
        if (countryPath is not null)
        {
            SetCurrentCountryPath(countryPath);
            ZoomAndPanToCountryCenter(countryPath);
            Pin.BeginAnimation();
            countryPath.Fill = ActiveFillColorBrush;
        }
    }

    private Path? GetCountryPath(string countryCode)
    {
        string elementName = GetCountryPathName(countryCode);
        return FindName(elementName) as Path;
    }

    private string GetCountryPathName(string countryCode)
    {
        return $"{COUNTRY_CODE_PREFIX}{countryCode.ToUpper()}";
    }

    private void SetCurrentCountryPath(Path countryPath)
    {
        _currentCountryPath = countryPath;
    }

    private void InvalidateCountryFill()
    {
        List<Path> countries = this.FindDescendants().OfType<Path>().ToList();
        foreach (Path country in countries)
        {
            country.Fill = InactiveFillColorBrush;
        }

        SetCurrentCountryFill();
    }

    private void SetCurrentCountryFill()
    {
        if (_currentCountryPath != null)
        {
            _currentCountryPath.Fill = ActiveFillColorBrush;
        }
    }
}