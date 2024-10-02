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

using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace ProtonVPN.Client.Legacy.UI.Home.Map;

public sealed partial class MapComponent
{
    private const int MAX_SCALE_FACTOR = 15;
    private const double PIN_WIDTH = 96;
    private const double PIN_HEIGHT = 96;
    private const string COUNTRY_CODE_PREFIX = "country_";

    private bool _capturingMouse;
    private double _currentScale = 1;

    private Point _startingMousePosition;
    private Path? _currentCountryPath;

    public static readonly DependencyProperty ActiveCountryCodeProperty = DependencyProperty.Register(
        nameof(ActiveCountryCode),
        typeof(string),
        typeof(MapComponent),
        new PropertyMetadata(null, OnActiveCountryCodeChanged));

    public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register(
        nameof(IsConnected),
        typeof(bool),
        typeof(MapComponent),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsConnectingProperty = DependencyProperty.Register(
        nameof(IsConnecting),
        typeof(bool),
        typeof(MapComponent),
        new PropertyMetadata(false));

    public static readonly DependencyProperty InactiveFillColorBrushProperty = DependencyProperty.Register(
        nameof(InactiveFillColorBrush),
        typeof(Brush),
        typeof(MapComponent),
        new PropertyMetadata(default, OnInactiveFillColorBrushChanged));

    public static readonly DependencyProperty ActiveFillColorBrushProperty = DependencyProperty.Register(
        nameof(ActiveFillColorBrush),
        typeof(Brush),
        typeof(MapComponent),
        new PropertyMetadata(default, OnActiveFillColorBrushChanged));

    public static readonly DependencyProperty IsCountryPinVisibleProperty = DependencyProperty.Register(
        nameof(IsCountryPinVisible),
        typeof(bool),
        typeof(MapComponent),
        new PropertyMetadata(true));

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

    public bool IsToShowPinAdjustmentUi => false;

    private double ViewportScaleX => Viewbox.ActualWidth / MapCanvas.Width;
    private double ViewportScaleY => Viewbox.ActualHeight / MapCanvas.Height;
    private double ViewportOffsetX => Viewbox.ActualWidth / 2 / ViewportScaleX;
    private double ViewportOffsetY => Viewbox.ActualHeight / 2 / ViewportScaleY;

    public MapComponent()
    {
        InitializeComponent();

        Pin.Width = PIN_WIDTH;
        Pin.Height = PIN_HEIGHT;
        PinTranslateTransform.X = -PIN_WIDTH;
        PinTranslateTransform.Y = -PIN_HEIGHT;

        Viewbox.SizeChanged += OnViewboxSizeChanged;

        //Uncomment for pin adjustment
        // PointerPressed += OnMousePressed;
        // PointerReleased += OnMouseReleased;
        // PointerMoved += OnMousePointerMoved;
    }

    private void OnViewboxSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_currentCountryPath is null)
        {
            return;
        }

        ZoomAndPanToCountryCenter(_currentCountryPath);
    }

    private void ZoomAndPanToCountryCenter(Path country)
    {
        double scale = GetScaleFactor(country);

        _currentScale = scale;

        ScaleMap(scale);

        string countryCode = GetCountryCode(country);
        if (_coordinates.ContainsKey(countryCode))
        {
            TranslateToPin(countryCode, scale);
        }
        else
        {
            TranslateToCountryCenter(country, scale);
        }

        LogPinOffset();
    }

    private double GetScaleFactor(Path country)
    {
        double divisor = GetCountryViewBoxDivisor(country);
        double maxCountryWidth = Viewbox.ActualWidth / divisor;
        double maxCountryHeight = Viewbox.ActualHeight / divisor;

        double width = (Viewbox.ActualWidth / MapCanvas.Width) * country.Data.Bounds.Width;
        double height = (Viewbox.ActualHeight / MapCanvas.Height) * country.Data.Bounds.Height;

        double scaleFactor = height > width ? maxCountryHeight / height : maxCountryWidth / width;

        return Math.Min(scaleFactor, MAX_SCALE_FACTOR);
    }

    private double GetCountryViewBoxDivisor(Path country)
    {
        double area = country.Data.Bounds.Width * country.Data.Bounds.Height;
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

        PinScaleTransform.ScaleX = (1 / scale) / ViewportScaleX;
        PinScaleTransform.ScaleY = (1 / scale) / ViewportScaleY;
    }

    private string GetCountryCode(Path country)
    {
        return country.Name.Replace(COUNTRY_CODE_PREFIX, "");
    }

    private void TranslateToPin(string countryCode, double scale)
    {
        double x = _coordinates[countryCode].X;
        double y = _coordinates[countryCode].Y;

        MapCanvasTranslateTransform.X = ViewportOffsetX - (x * scale - Pin.ActualWidth / 2);
        MapCanvasTranslateTransform.Y = ViewportOffsetY - (y * scale - Pin.ActualHeight / 2);

        PinTranslateTransform.X = x - (Pin.ActualWidth / 2 / scale) / ViewportScaleX;
        PinTranslateTransform.Y = y - (Pin.ActualHeight / 2 / scale) / ViewportScaleY;
    }

    private void TranslateToCountryCenter(Path country, double scale)
    {
        double countryOffsetX = (country.Data.Bounds.Left + country.Data.Bounds.Width / 2) * scale;
        double countryOffsetY = (country.Data.Bounds.Top + country.Data.Bounds.Height / 2) * scale;

        MapCanvasTranslateTransform.X = ViewportOffsetX - countryOffsetX;
        MapCanvasTranslateTransform.Y = ViewportOffsetY - countryOffsetY;

        PinTranslateTransform.X = country.Data.Bounds.Left + (country.Data.Bounds.Width / 2) - (Pin.ActualWidth / 2 / scale) / ViewportScaleX;
        PinTranslateTransform.Y = country.Data.Bounds.Top + (country.Data.Bounds.Height / 2) - (Pin.ActualHeight / 2 / scale) / ViewportScaleY;
    }

    [Conditional("DEBUG")]
    private void LogPinOffset()
    {
        PinOffsetX.Text = (PinTranslateTransform.X + Pin.ActualWidth / 2 / _currentScale).ToString(CultureInfo.InvariantCulture);
        PinOffsetY.Text = (PinTranslateTransform.Y + Pin.ActualHeight / 2 / _currentScale).ToString(CultureInfo.InvariantCulture);
    }

    private static void OnActiveCountryCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string countryCode || d is not MapComponent control)
        {
            return;
        }

        string? previousCountryCode = (string)e.OldValue;
        if (previousCountryCode is not null)
        {
            control.MakeCountryInactive(previousCountryCode);
        }

        if (!string.IsNullOrEmpty(countryCode))
        {
            Path? countryPath = control.GetCountryPath(countryCode);
            if (countryPath is not null)
            {
                control.SetCurrentCountryPath(countryPath);
                control.ZoomAndPanToCountryCenter(countryPath);
                control.Pin.BeginAnimation();
                countryPath.Fill = control.ActiveFillColorBrush;
            }
        }
    }

    private void MakeCountryInactive(string countryCode)
    {
        Path? countryPath = GetCountryPath(countryCode);
        if (countryPath is not null)
        {
            countryPath.Fill = InactiveFillColorBrush;
        }
    }

    private Path? GetCountryPath(string countryCode)
    {
        string elementName = GetCountryPathName(countryCode);
        return FindName(elementName) as Path;
    }

    private string GetCountryPathName(string countryCode)
    {
        return $"country_{countryCode.ToUpper()}";
    }

    private void SetCurrentCountryPath(Path countryPath)
    {
        _currentCountryPath = countryPath;
    }

    private void ZoomToCountry(object sender, RoutedEventArgs e)
    {
        ActiveCountryCode = CountryTextBox.Text;
        IsConnected = true;
    }

    private void OnMousePressed(object sender, PointerRoutedEventArgs e)
    {
        _capturingMouse = true;
    }

    private void OnMousePointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_capturingMouse)
        {
            return;
        }

        Point pointerPosition = e.GetCurrentPoint(null).Position;
        double mouseOffsetX = pointerPosition.X.CompareTo(_startingMousePosition.X) / _currentScale;
        double mouseOffsetY = pointerPosition.Y.CompareTo(_startingMousePosition.Y) / _currentScale;

        PinTranslateTransform.X += mouseOffsetX;
        PinTranslateTransform.Y += mouseOffsetY;

        LogPinOffset();

        _startingMousePosition = pointerPosition;
    }

    private void OnMouseReleased(object sender, PointerRoutedEventArgs e)
    {
        _capturingMouse = false;
        _startingMousePosition = new();
    }

    private static void OnInactiveFillColorBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapComponent control)
        {
            return;
        }

        control.InvalidateCountryFill();
    }

    private static void OnActiveFillColorBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapComponent control)
        {
            return;
        }

        control.InvalidateCountryFill();
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