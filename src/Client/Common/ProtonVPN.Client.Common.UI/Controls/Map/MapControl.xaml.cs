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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Mapsui;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using NetTopologySuite.Geometries;
using ProtonVPN.Client.Common.Helpers;
using ProtonVPN.Client.Common.UI.Controls.Map.Animations;
using ProtonVPN.Client.Common.UI.Controls.Map.Extensions;
using ProtonVPN.Client.Common.UI.Controls.Map.Providers;
using Brush = Mapsui.Styles.Brush;

namespace ProtonVPN.Client.Common.UI.Controls.Map;

public sealed partial class MapControl
{
    // Resolution related consts
    private const int SIDE_MARGIN = 8;
    private const double MIN_BBOX_WIDTH = 4411437;
    private const double MIN_BBOX_HEIGHT = 2445113; 
    private const double ZOOMED_IN_WORLD_RATIO = 0.10; // 10% of the world map
    private const double ZOOMED_OUT_WORLD_RATIO = 0.80; // 80% of the world map
    private const int ZOOM_LEVELS_COUNT = 10; // Number of zoom level

    private static readonly double[] _zoomLevels = GenerateZoomLevels(ZOOMED_IN_WORLD_RATIO, ZOOMED_OUT_WORLD_RATIO, ZOOM_LEVELS_COUNT);

    // Animations related consts
    private const int ZOOM_ANIMATION_DURATION_IN_MS = 1200;
    private const int PINS_LAYER_ANIMATION_DURATION_IN_MS = 200;
    private const int COUNTRY_CHANGE_ANIMATION_DURATION_IN_MS = 1000;
    private const int MAX_PIN_PULSE_COUNT = 3;
    private const int PIN_PULSE_DURATION_IN_MS = 1300;
    private const int PIN_PULSE_DELAY_IN_MS = 700;
    private const float PIN_PULSE_SHRINK_RATIO = 0.5f;
    private const float PIN_PULSE_DELAY_RATIO = PIN_PULSE_DELAY_IN_MS / PIN_PULSE_DURATION_IN_MS;

    public static readonly DependencyProperty CountriesProperty = DependencyProperty.Register(
        nameof(Countries),
        typeof(List<Country>),
        typeof(MapControl),
        new PropertyMetadata(new List<Country>(), OnCountriesPropertyChanged));

    public static readonly DependencyProperty CurrentCountryProperty = DependencyProperty.Register(
        nameof(CurrentCountry),
        typeof(Country),
        typeof(MapControl),
        new PropertyMetadata(null, OnCurrentCountryChanged));

    public static readonly DependencyProperty ConnectCommandProperty = DependencyProperty.Register(
        nameof(ConnectCommand),
        typeof(ICommand),
        typeof(MapControl),
        new PropertyMetadata(default));

    public static readonly DependencyProperty IsConnectingProperty = DependencyProperty.Register(
        nameof(IsConnecting),
        typeof(bool),
        typeof(MapControl),
        new PropertyMetadata(false, OnIsConnectingChanged));

    public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register(
        nameof(IsConnected),
        typeof(bool),
        typeof(MapControl),
        new PropertyMetadata(false, OnIsConnectedChanged));

    public static readonly DependencyProperty IsDisconnectedProperty = DependencyProperty.Register(
        nameof(IsDisconnected),
        typeof(bool),
        typeof(MapControl),
        new PropertyMetadata(true, OnIsDisconnectedChanged));

    public static readonly DependencyProperty LeftOffsetProperty = DependencyProperty.Register(
        nameof(LeftOffset),
        typeof(double),
        typeof(MapControl),
        new PropertyMetadata(default, OnMapOffsetChanged));

    public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register(
        nameof(TopOffset),
        typeof(double),
        typeof(MapControl),
        new PropertyMetadata(default, OnMapOffsetChanged));

    public static readonly DependencyProperty RightOffsetProperty = DependencyProperty.Register(
        nameof(RightOffset),
        typeof(double),
        typeof(MapControl),
        new PropertyMetadata(default, OnMapOffsetChanged));

    public static readonly DependencyProperty BottomOffsetProperty = DependencyProperty.Register(
        nameof(BottomOffset),
        typeof(double),
        typeof(MapControl),
        new PropertyMetadata(default, OnMapOffsetChanged));

    public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
        nameof(TitleBarHeight),
        typeof(double),
        typeof(MapControl),
        new PropertyMetadata(default, OnMapOffsetChanged));

    public static readonly DependencyProperty ConnectPhraseProperty = DependencyProperty.Register(
        nameof(ConnectPhrase),
        typeof(string),
        typeof(MapControl),
        new PropertyMetadata(default, OnConnectPhraseChanged));

    public List<Country> Countries
    {
        get => (List<Country>)GetValue(CountriesProperty);
        set => SetValue(CountriesProperty, value);
    }

    public Country? CurrentCountry
    {
        get => (Country?)GetValue(CurrentCountryProperty);
        set => SetValue(CurrentCountryProperty, value);
    }

    public ICommand ConnectCommand
    {
        get => (ICommand)GetValue(ConnectCommandProperty);
        set => SetValue(ConnectCommandProperty, value);
    }

    public bool IsConnecting
    {
        get => (bool)GetValue(IsConnectingProperty);
        set => SetValue(IsConnectingProperty, value);
    }

    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public bool IsDisconnected
    {
        get => (bool)GetValue(IsDisconnectedProperty);
        set => SetValue(IsDisconnectedProperty, value);
    }

    public double LeftOffset
    {
        get => (double)GetValue(LeftOffsetProperty);
        set => SetValue(LeftOffsetProperty, value);
    }

    public double TopOffset
    {
        get => (double)GetValue(TopOffsetProperty);
        set => SetValue(TopOffsetProperty, value);
    }

    public double RightOffset
    {
        get => (double)GetValue(RightOffsetProperty);
        set => SetValue(RightOffsetProperty, value);
    }

    public double BottomOffset
    {
        get => (double)GetValue(BottomOffsetProperty);
        set => SetValue(BottomOffsetProperty, value);
    }
    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    public string ConnectPhrase
    {
        get => (string)GetValue(ConnectPhraseProperty);
        set => SetValue(ConnectPhraseProperty, value);
    }

    private static Color _defaultFillColor = Color.FromArgb(255, 53, 49, 64);
    private static Color _defaultOutlineColor = Color.FromArgb(255, 103, 98, 120);
    private static int _defaultOutlineThickness = 1;

    private readonly VectorStyle _countriesStyle = new()
    {
        Fill = new Brush(_defaultFillColor),
        Outline = new Pen(_defaultOutlineColor, _defaultOutlineThickness),
    };

    private readonly VectorStyle _disputedTerritoriesStyle = new()
    {
        Fill = new Brush(_defaultFillColor),
        Outline = new Pen(_defaultOutlineColor, _defaultOutlineThickness)
        {
            PenStyle = PenStyle.Dash
        },
        Line = new Pen(_defaultOutlineColor, _defaultOutlineThickness)
        {
            PenStyle = PenStyle.Dash
        },
    };

    private readonly VectorStyle _statesStyle = new()
    {
        Line = new Pen(_defaultOutlineColor, _defaultOutlineThickness)
        {
            PenStyle = PenStyle.Dash
        },
    };

    private MRect _mapBounds = new(
        -1.95e7, // Min X - West
        -0.70e7, // Min Y - South
        +1.95e7, // Max X - East
        +1.75e7  // Max Y - North
    );

    private readonly Mapsui.Map _map = new()
    {
        CRS = "EPSG:3857",
    };

    private bool _isManipulationRunning;
    private bool _isUnloaded;
    private bool _arePinsLoaded;

    private ILayer? _pinsLayer;
    private ILayer? _countriesLayer;
    private CountryCallout _countryCallout;
    private GeometryFeature? _selectedPin;
    private Windows.Foundation.Point _currentMousePosition;

    private AnimationEntry<double>? _pinFadeAnimation;
    private readonly ConcurrentDictionary<AnimatedCircleStyle, List<AnimationEntry<AnimatedCircleStyle>>> _activeAnimations = [];
    private readonly AnimatedCirclesStyleSkiaRenderer _animatedCirclesStyleSkiaRenderer = new();

    private readonly InputSystemCursor _handCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

    public MapControl()
    {
        InitializeComponent();

        _countryCallout = new CountryCallout();
        _countryCallout.LayoutUpdated += OnCountryCalloutLayoutUpdated;

        _countriesLayer = GetWorldMapLayerAsync().Result;
        _map.Layers.Add(_countriesLayer);
        _map.Layers.Add(GetDisputedTerritoriesLayer());

        // TOOD: implement states
        //_map.Layers.Add(GetUsaStatesLayer());

        _map.Navigator.OverridePanBounds = _mapBounds;
        _map.Navigator.MouseWheelAnimation.Duration = ZOOM_ANIMATION_DURATION_IN_MS;
        _map.Home = (Navigator n) => n.ZoomToBox(_mapBounds);

        _map.Info += HandleMapClick;
        _map.Navigator.ViewportChanged += OnViewportChanged;
        MapCanvas.Children.Add(_countryCallout);
        Map.Renderer.StyleRenderers.Add(typeof(AnimatedCircleStyle), _animatedCirclesStyleSkiaRenderer);

        Map.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateInertia;

        Map.Map = _map;
        Map.Loaded += OnMapLoaded;
        Map.Unloaded += OnMapUnloaded;
    }

    private void OnMapLoaded(object sender, RoutedEventArgs e)
    {
        _isUnloaded = false;

        Map.PointerPressed += OnPointerPressed;
        Map.PointerMoved += OnPointerMoved;
        Map.ManipulationStarted += OnManipulationStarted;
        Map.ManipulationInertiaStarting += OnManipulationInertiaStarted;
        Map.PointerReleased += OnPointerReleased;
        Map.SizeChanged += OnMapSizeChanged;

        ActualThemeChanged += OnThemeChanged;

        InvalidateTheme();
    }

    private void OnMapUnloaded(object sender, RoutedEventArgs e)
    {
        _isUnloaded = true;

        Map.PointerPressed -= OnPointerPressed;
        Map.PointerMoved -= OnPointerMoved;
        Map.ManipulationStarted -= OnManipulationStarted;
        Map.ManipulationInertiaStarting -= OnManipulationInertiaStarted;
        Map.PointerReleased -= OnPointerReleased;
        Map.SizeChanged -= OnMapSizeChanged;

        ActualThemeChanged -= OnThemeChanged;
    }

    private void OnThemeChanged(FrameworkElement sender, object args)
    {
        InvalidateTheme();
        InvalidatePins();
    }

    private void InvalidateTheme()
    {
        if (Map.Background is SolidColorBrush solidColorBrush)
        {
            Windows.UI.Color winUIColor = solidColorBrush.Color;
            _map.BackColor = Color.FromArgb(winUIColor.A, winUIColor.R, winUIColor.G, winUIColor.B);
        }

        if (Resources.TryGetValue("CountryFillBrush", out object? fillResource) &&
            fillResource is SolidColorBrush fillBrush &&
            Resources.TryGetValue("CountryOutlineBrush", out object? outlineResource) &&
            outlineResource is SolidColorBrush outlineBrush)
        {
            Color fillColor = Color.FromArgb(fillBrush.Color.A, fillBrush.Color.R, fillBrush.Color.G, fillBrush.Color.B);
            Color outlineColor = Color.FromArgb(outlineBrush.Color.A, outlineBrush.Color.R, outlineBrush.Color.G, outlineBrush.Color.B);

            _countriesStyle.Fill = new Brush(fillColor);
            _countriesStyle.Outline = new Pen(outlineColor, _defaultOutlineThickness);

            _disputedTerritoriesStyle.Fill = new Brush(fillColor);
            _disputedTerritoriesStyle.Outline = new Pen(outlineColor, _defaultOutlineThickness)
            {
                PenStyle = PenStyle.Dash
            };
            _disputedTerritoriesStyle.Line = new Pen(outlineColor, _defaultOutlineThickness)
            {
                PenStyle = PenStyle.Dash
            };

            _statesStyle.Line = new Pen(outlineColor, _defaultOutlineThickness)
            {
                PenStyle = PenStyle.Dash
            };
        }

        _animatedCirclesStyleSkiaRenderer.ElementTheme = ActualTheme;
    }

    private void InvalidateZoomLevels()
    {
        double worldResolution = ZoomHelper.CalculateResolutionForWorldSize(
            _mapBounds.Width,
            _mapBounds.Height,
            _map.Navigator.Viewport.Width,
            _map.Navigator.Viewport.Height);

        double fullyZoomedInResolution = worldResolution * ZOOMED_IN_WORLD_RATIO;
        double fullyZoomedOutResolution = worldResolution * ZOOMED_OUT_WORLD_RATIO;

        List<double> resolutions = _zoomLevels
            .Select(level => worldResolution * level)
            .ToList();

        _map.Navigator.OverrideZoomBounds = new MMinMax(
            fullyZoomedInResolution,
            fullyZoomedOutResolution);

        _map.Navigator.OverrideResolutions = resolutions;
    }

    private static double[] GenerateZoomLevels(double minZoom, double maxZoom, int count, double exponent = 1.5)
    {
        if (count < 2)
        {
            return [];
        }

        double[] levels = new double[count];

        for (int i = 0; i < count; i++)
        {
            double t = (double)i / (count - 1);
            double adjusted = Math.Pow(t, exponent);
            double zoom = minZoom + (maxZoom - minZoom) * adjusted;
            levels[i] = Math.Round(zoom, 2);
        }

        Array.Reverse(levels);

        return levels;
    }

    private void OnMapSizeChanged(object sender, SizeChangedEventArgs e)
    {
        InvalidateCurrentCountry();
    }

    private void HandleMapClick(object? s, MapInfoEventArgs a)
    {
        if (a.MapInfo?.WorldPosition == null)
        {
            return;
        }

        MRect searchArea = GetMouseBufferBounds();
        List<IFeature> pins = GetAllPins();
        foreach (GeometryFeature feature in pins.Cast<GeometryFeature>())
        {
            if (feature.Geometry is Point point &&
                searchArea.Contains(point.ToMPoint()) &&
                Countries.FirstOrDefault(c => c.Code == feature.GetCountryCode()) is Country country)
            {
                ConnectCommand.Execute(country);
                break;
            }
        }
    }

    private MRect GetMouseBufferBounds()
    {
        MPoint mousePosition = _map.Navigator.Viewport.ScreenToWorld(_currentMousePosition.X, _currentMousePosition.Y);
        double buffer = 15 * _map.Navigator.Viewport.Resolution;

        return new MRect(
            mousePosition.X - buffer,
            mousePosition.Y - buffer,
            mousePosition.X + buffer,
            mousePosition.Y + buffer
        );
    }

    private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        _isManipulationRunning = true;
        _countryCallout.Hide();
    }

    private void OnManipulationInertiaStarted(object sender, ManipulationInertiaStartingRoutedEventArgs e)
    {
        Windows.Foundation.Point velocity = e.Velocities.Linear; // Velocity in pixels/ms
        double velocityLength = Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);

        // Parameters for scaling
        double k = 0.001; // Tuning factor
        double exp = 1.5; // Exponent to amplify high velocities

        // Non-linear scaled deceleration
        double deceleration = k * Math.Pow(velocityLength, exp);

        // Clamp to avoid too small or too high deceleration
        double minDeceleration = 0.005; // px/ms²
        double maxDeceleration = 0.2;   // px/ms²

        deceleration = Math.Clamp(deceleration, minDeceleration, maxDeceleration);

        e.TranslationBehavior.DesiredDeceleration = deceleration;

        e.Handled = true;
    }

    private void OnCountryCalloutLayoutUpdated(object? sender, object e)
    {
        if (_countryCallout.Country == null)
        {
            return;
        }

        MPoint point = _countryCallout.Country.GetMapPoint();
        MPoint screenPosition = _map.Navigator.Viewport.WorldToScreen(point);

        double left = screenPosition.X - _countryCallout.ActualWidth / 2.0;
        double top = screenPosition.Y - _countryCallout.ActualHeight - 20;

        Canvas.SetLeft(_countryCallout, left);
        Canvas.SetTop(_countryCallout, top);
    }

    private void OnViewportChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isManipulationRunning)
        {
            return;
        }

        InvalidateZoomLevels();

        DispatcherQueue?.TryEnqueue(ToggleFeatures);
    }

    private bool _isMouseOnViewport;
    private bool _fadeInAnimationStarted;
    private bool _fadeOutAnimationStarted;

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        Map.CapturePointer(e.Pointer);
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isManipulationRunning || _isUnloaded)
        {
            return;
        }

        _currentMousePosition = e.GetCurrentPoint(Map).Position;

        _isMouseOnViewport = IsMouseOnViewport(_currentMousePosition);

        if (!_fadeInAnimationStarted && _isMouseOnViewport)
        {
            _fadeInAnimationStarted = true;
            _fadeOutAnimationStarted = false;
            StartFadeInPinAnimation();
        }

        if (_fadeInAnimationStarted && !_fadeOutAnimationStarted && !_isMouseOnViewport)
        {
            _fadeOutAnimationStarted = true;
            _fadeInAnimationStarted = false;
            StartFadeOutPinAnimation();
        }

        ToggleFeatures();
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        Map.ReleasePointerCapture(e.Pointer);
        _isManipulationRunning = false;
    }

    private bool IsMouseOnViewport(Windows.Foundation.Point mousePosition)
    {
        double left = SIDE_MARGIN + LeftOffset;
        double top = TitleBarHeight + TopOffset;
        double right = SIDE_MARGIN;
        double bottom = BottomOffset + SIDE_MARGIN;

        Windows.Foundation.Rect viewportRect = new(
            left,
            top,
            Map.ActualWidth - (left + right),
            Map.ActualHeight - (top + bottom)
        );

        return viewportRect.Contains(mousePosition);
    }

    private void ToggleFeatures()
    {
        if (_isUnloaded)
        {
            return;
        }

        MRect searchArea = GetMouseBufferBounds();
        MPoint mousePosition = new(_currentMousePosition.X, _currentMousePosition.Y);

        IEnumerable<IFeature> features = GetPinFeature(searchArea);

        GeometryFeature? closestFeature = null;
        double minDistance = double.MaxValue;

        foreach (GeometryFeature feature in features.Cast<GeometryFeature>())
        {
            if (feature.Geometry is Point point)
            {
                MPoint pinPosition = point.ToMPoint();
                if (searchArea.Contains(pinPosition))
                {
                    double distance = mousePosition.Distance(pinPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestFeature = feature;
                    }
                }
            }
        }

        if (closestFeature == null || !_isMouseOnViewport)
        {
            if (_selectedPin != null)
            {
                _selectedPin.SetIsOnHover(false);
                StartPinScaleAnimations(_selectedPin, _selectedPin.GetHoverLostAnimationType());
            }
            _selectedPin = null;

            foreach (GeometryFeature feature in features.Cast<GeometryFeature>().Where(f => f.IsOnHover()))
            {
                feature.SetIsOnHover(false);
                StartPinScaleAnimations(feature, feature.GetHoverLostAnimationType());
            }

            OverrideCursor(null);
            _countryCallout.Hide();
        }
        else if (closestFeature != _selectedPin)
        {
            if (_selectedPin != null)
            {
                _selectedPin.SetIsOnHover(false);
                StartPinScaleAnimations(_selectedPin, _selectedPin.GetHoverLostAnimationType());
            }

            _selectedPin = closestFeature;
            ActivateCountry(_selectedPin);
        }

        _map.RefreshGraphics();
    }

    private void ActivateCountry(GeometryFeature feature)
    {
        if (feature.IsConnecting())
        {
            return;
        }

        string? countryCode = feature.GetCountryCode();
        if (!string.IsNullOrEmpty(countryCode))
        {
            Country? country = Countries.FirstOrDefault(c => c.Code == countryCode);
            if (country != null)
            {
                feature.SetIsOnHover(true);

                _countryCallout.Show(country);

                OverrideCursor(_handCursor);

                StartPinScaleAnimations(feature, feature.GetOnHoverAnimationType());

                return;
            }
        }
    }

    private IEnumerable<IFeature> GetPinFeature(MRect searchArea)
    {
        return _pinsLayer?.GetFeatures(searchArea, _map.Navigator.Viewport.Resolution) ?? [];
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);

        if (_fadeInAnimationStarted)
        {
            StartFadeOutPinAnimation();
        }

        _fadeInAnimationStarted = false;
        _fadeOutAnimationStarted = false;

        _countryCallout.Hide();
    }

    private string GetDataAssetPath(string asset)
    {
        return AssetPathHelper.GetAbsoluteAssetPath("Map", "Data", asset);
    }

    private async Task<ILayer> GetWorldMapLayerAsync()
    {
        CustomGeoJsonProvider geoJsonProvider = new(new GeoJsonProvider(File.ReadAllText(GetDataAssetPath("Map.geojson"))))
        {
            CRS = "EPSG:4326",
        };

        ProjectingProvider projectingProvider = new(geoJsonProvider)
        {
            CRS = "EPSG:3857",
        };

        IEnumerable<IFeature> features = await projectingProvider.GetFeaturesAsync(new FetchInfo(new MSection(_mapBounds, 1)));

        return new Layer
        {
            Name = "Countries",
            DataSource = new CustomMemoryProvider(features),
            Style = _countriesStyle,
            IsMapInfoLayer = true,
        };
    }

    private ILayer GetDisputedTerritoriesLayer()
    {
        CustomGeoJsonProvider geoJsonProvider = new(new GeoJsonProvider(File.ReadAllText(GetDataAssetPath("MapDisputedAreas.geojson"))))
        {
            CRS = "EPSG:4326",
        };

        return new Layer
        {
            Name = "Disputed territories",
            DataSource = new ProjectingProvider(geoJsonProvider)
            {
                CRS = "EPSG:3857",
            },
            Style = _disputedTerritoriesStyle,
            IsMapInfoLayer = true
        };
    }

    private ILayer GetUsaStatesLayer()
    {
        CustomGeoJsonProvider geoJsonProvider = new(new GeoJsonProvider(File.ReadAllText(GetDataAssetPath("UsaStates.geojson"))))
        {
            CRS = "EPSG:4326",
        };

        return new Layer
        {
            Name = "US states",
            DataSource = new ProjectingProvider(geoJsonProvider)
            {
                CRS = "EPSG:3857",
            },
            Style = _statesStyle,
            IsMapInfoLayer = true
        };
    }

    private ILayer GetPinsLayer(IEnumerable<IFeature> pins)
    {
        Layer layer = new()
        {
            Name = "Country pins",
            DataSource = new CustomMemoryProvider(pins),
            Style = new VectorStyle()
            {
                Outline = new Pen
                {
                    Color = Color.Transparent,
                    Width = 0
                },
                Line = new Pen
                {
                    Color = Color.Transparent,
                    Width = 0
                },
                Fill = new Brush
                {
                    Color = Color.Transparent
                },
            },
            Enabled = true,
            Opacity = 0,
        };

        layer.Animations.Add(OnLayerAnimationTick);

        return layer;
    }

    private bool OnLayerAnimationTick()
    {
        if (_activeAnimations.Count <= 0 && _pinFadeAnimation is null)
        {
            return false;
        }

        bool isAnimationRunning = false;

        foreach (KeyValuePair<AnimatedCircleStyle, List<AnimationEntry<AnimatedCircleStyle>>> kvp in _activeAnimations.ToList())
        {
            AnimatedCircleStyle style = kvp.Key;
            List<AnimationEntry<AnimatedCircleStyle>> entries = kvp.Value;

            AnimationResult<AnimatedCircleStyle> result = Animation.UpdateAnimations(style, entries);
            if (result.IsRunning)
            {
                isAnimationRunning = true;
            }
            else
            {
                _activeAnimations.TryRemove(style, out _);
            }
        }

        if (_pinFadeAnimation is not null)
        {
            AnimationResult<double> result = Animation.UpdateAnimations(_pinFadeAnimation.AnimationEnd, [_pinFadeAnimation]);
            if (result.IsRunning)
            {
                isAnimationRunning = true;
            }
            else
            {
                _pinFadeAnimation = null;
            }
        }

        return isAnimationRunning;
    }

    private AnimationEntry<AnimatedCircleStyle>? CreateShrinkGrowCycles(AnimatedCircleStyle style, int remainingPairs)
    {
        if (remainingPairs <= 0)
        {
            return null;
        }

        AnimationEntry<AnimatedCircleStyle>? nextShrinkAnimation = CreateShrinkGrowCycles(style, remainingPairs - 1);

        AnimationEntry<AnimatedCircleStyle> growAnimation = CreateAnimationEntry(
            AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE * PIN_PULSE_SHRINK_RATIO,
            AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
            PIN_PULSE_DELAY_RATIO,
            1f,
            Easing.CubicInOut,
            (s, val) => style.TransparentCircleRadius = val,
            nextShrinkAnimation != null
                ? new SubsequentAnimation
                {
                    AnimatedCircleStyle = style,
                    Animation = nextShrinkAnimation,
                    TotalDurationInMs = PIN_PULSE_DURATION_IN_MS,
                }
                : null
        );

        AnimationEntry<AnimatedCircleStyle> shrinkAnimation = CreateAnimationEntry(
            AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE,
            AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE * PIN_PULSE_SHRINK_RATIO,
            PIN_PULSE_DELAY_RATIO,
            1f,
            Easing.CubicInOut,
            (s, val) => style.TransparentCircleRadius = val,
            new SubsequentAnimation
            {
                AnimatedCircleStyle = style,
                Animation = growAnimation,
                TotalDurationInMs = PIN_PULSE_DURATION_IN_MS,
            }
        );

        return shrinkAnimation;
    }

    private List<AnimationEntry<AnimatedCircleStyle>> CreatePinAnimations(IFeature feature, AnimatedCircleStyle style, PinAnimationType pinAnimationType)
    {
        PinAnimationConfiguration animationConfig = PinAnimationConfiguration.Get(pinAnimationType);

        SubsequentAnimation? subsequentAnimation = pinAnimationType.IsSubsequentPulsatingAnimationRequired(feature)
            ? new SubsequentAnimation()
            {
                Animation = CreateShrinkGrowCycles(style, MAX_PIN_PULSE_COUNT),
                AnimatedCircleStyle = style,
                TotalDurationInMs = PIN_PULSE_DURATION_IN_MS,
            }
            : null;

        return [
            CreateAnimationEntry(
                animationConfig.TransparentCircle.Start,
                animationConfig.TransparentCircle.End,
                0,
                1,
                Easing.Linear,
                (s, val) => style.TransparentCircleRadius = val,
                subsequentAnimation),
            CreateAnimationEntry(
                animationConfig.NeutralCircle.Start,
                animationConfig.NeutralCircle.End,
                0,
                1,
                Easing.Linear,
                (s, val) => style.NeutralCircleRadius = val),
            CreateAnimationEntry(
                animationConfig.CenterCircle.Start,
                animationConfig.CenterCircle.End,
                0,
                1,
                Easing.Linear,
                (s, val) => style.CenterCircleRadius = val),
        ];
    }

    private AnimationEntry<AnimatedCircleStyle> CreateAnimationEntry<T>(
        T start,
        T end,
        float animationStart,
        float animationEnd,
        Easing easing,
        Action<AnimatedCircleStyle, T> updateAction,
        SubsequentAnimation? subsequentAnimation = null)
        where T : INumber<T>
    {
        return new AnimationEntry<AnimatedCircleStyle>(
            start,
            end,
            animationStart,
            animationEnd,
            easing,
            repeat: false,
            tick: (symbolStyle, e, v) =>
            {
                T factor = T.CreateChecked(e.Easing.Ease(v));
                T interpolated = (T)e.Start + ((T)e.End - (T)e.Start) * factor;
                updateAction(symbolStyle, interpolated);
                return new AnimationResult<AnimatedCircleStyle>(symbolStyle, true);
            },
            final: (symbolStyle, e) =>
            {
                updateAction(symbolStyle, (T)e.End);
                _map.RefreshGraphics();

                DispatcherQueue?.TryEnqueue(() =>
                {
                    if (subsequentAnimation?.Animation != null)
                    {
                        Animation.Start(subsequentAnimation.Animation, subsequentAnimation.TotalDurationInMs);
                        _activeAnimations.AddOrUpdate(subsequentAnimation.AnimatedCircleStyle, _ => [subsequentAnimation.Animation], (_, _) => [subsequentAnimation.Animation]);
                    }
                });

                return new AnimationResult<AnimatedCircleStyle>(symbolStyle, false);
            });
    }

    private static void OnCountriesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        control.InvalidatePins();
    }

    private static void OnCurrentCountryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        control.InvalidateCurrentCountry(COUNTRY_CHANGE_ANIMATION_DURATION_IN_MS);
    }

    private static void OnIsConnectingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control || !(bool)e.NewValue)
        {
            return;
        }

        control.HandleConnectingState();
    }

    private static void OnIsDisconnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control || !(bool)e.NewValue)
        {
            return;
        }

        control.HandleDisconnectedState();
    }

    private static void OnMapOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        control.InvalidateCurrentCountry(COUNTRY_CHANGE_ANIMATION_DURATION_IN_MS);
    }

    private static void OnConnectPhraseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control)
        {
            return;
        }

        control.InvalidateConnectPhrase();
    }

    private void HandleDisconnectedState()
    {
        List<IFeature> pins = GetAllPins();

        foreach (IFeature pin in pins)
        {
            if (pin.GetCountryCode() == CurrentCountry?.Code)
            {
                pin.SetIsCurrentCountry(true);

                if (!pin.IsConnected())
                {
                    StartPinScaleAnimations(pin, PinAnimationType.DisconnectedToCurrentLocation);
                }
            }
            else
            {
                pin.SetIsCurrentCountry(false);
                StartPinScaleAnimations(pin, pin.GetDisconnectedAnimationType());
            }

            pin.SetIsConnected(false);
            pin.SetIsConnecting(false);
        }
    }

    private void HandleConnectingState()
    {
        List<IFeature> pins = GetAllPins();
        foreach (IFeature pin in pins)
        {
            if (pin.GetCountryCode() == CurrentCountry?.Code)
            {
                PinAnimationType animation = pin.GetConnectingAnimationType();
                StartPinScaleAnimations(pin, animation);

                pin.SetIsConnecting(true);
            }
            else
            {
                PinAnimationType? pinAnimationType = pin.GetDisconnectedAnimationType();

                pin.SetIsConnecting(false);
                pin.SetIsCurrentCountry(false);

                if (pinAnimationType != null)
                {
                    StartPinScaleAnimations(pin, pinAnimationType);
                }
            }

            pin.SetIsConnected(false);
            pin.SetIsOnHover(false);
        }
    }

    private static void OnIsConnectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MapControl control || !(bool)e.NewValue)
        {
            return;
        }

        control.HandleConnectedState();
    }

    private void HandleConnectedState()
    {
        List<IFeature> pins = GetAllPins();
        foreach (IFeature pin in pins)
        {
            if (pin.GetCountryCode() == CurrentCountry?.Code)
            {
                pin.SetIsConnected(true);
                pin.SetIsCurrentCountry(true);

                StartPinScaleAnimations(pin, PinAnimationType.ConnectingToConnected);
            }
            else
            {
                if (pin.IsCurrentCountry())
                {
                    pin.SetIsCurrentCountry(false);
                    StartPinScaleAnimations(pin, PinAnimationType.NormalLocationConnectingToDisconnected);
                }

                pin.SetIsConnected(false);
            }

            pin.SetIsConnecting(false);
        }
    }

    private void InvalidatePins()
    {
        List<IFeature> features = [];

        foreach (Country country in Countries)
        {
            IFeature feature = new GeometryFeature()
            {
                Geometry = country.GetMapPoint().ToPoint(),
            };

            AnimatedCircleStyle style = new();
            bool isCurrentCountry = country.Code == CurrentCountry?.Code;
            if (isCurrentCountry)
            {
                style.TransparentCircleRadius = AnimatedCircleStyle.TRANSPARENT_CIRCLE_RADIUS_ACTIVE;
                style.NeutralCircleRadius = AnimatedCircleStyle.NEUTRAL_CIRCLE_RADIUS_ACTIVE;
                style.CenterCircleRadius = AnimatedCircleStyle.CENTER_CIRCLE_RADIUS_ACTIVE;
            }

            feature.SetCountryCode(country.Code);
            feature.SetIsUnderMaintenance(country.IsUnderMaintenance);
            feature.SetIsCurrentCountry(isCurrentCountry);
            feature.SetIsConnected(IsConnected && isCurrentCountry);
            feature.Styles.Add(style);
            features.Add(feature);
        }

        if (_pinsLayer != null)
        {
            _pinsLayer.DataChanged -= OnPinsLayerDataChanged;
            _map.Layers.Remove(_pinsLayer);
        }

        _pinsLayer = GetPinsLayer(features);
        _pinsLayer.DataChanged += OnPinsLayerDataChanged;

        _map.Layers.Add(_pinsLayer);
    }

    private void OnPinsLayerDataChanged(object sender, Mapsui.Fetcher.DataChangedEventArgs e)
    {
        if (_arePinsLoaded)
        {
            return;
        }

        _arePinsLoaded = true;

        DispatcherQueue?.TryEnqueue(() =>
        {
            if (IsDisconnected)
            {
                HandleDisconnectedState();
            }
        });
    }

    private double CalculateResolution(MRect bbox, double viewportWidth, double viewportHeight, double marginFactor)
    {
        double effectiveWidth = Math.Max(MIN_BBOX_WIDTH, bbox.Width);
        double effectiveHeight = Math.Max(MIN_BBOX_HEIGHT, bbox.Height);

        double resolutionX = effectiveWidth / viewportWidth;
        double resolutionY = effectiveHeight / viewportHeight;
        double requiredResolution = Math.Max(resolutionX, resolutionY);

        return requiredResolution * marginFactor;
    }

    private void InvalidateCurrentCountry(long animationDuration = 0)
    {
        if (CurrentCountry == null || _countriesLayer == null)
        {
            return;
        }

        MPoint countryPoint = CurrentCountry.GetMapPoint();

        double leftOffset = SIDE_MARGIN + LeftOffset;
        double topOffset = TitleBarHeight + TopOffset;
        double rightOffset = RightOffset + SIDE_MARGIN;
        double bottomOffset = BottomOffset + SIDE_MARGIN;

        double effectiveViewportWidth = _map.Navigator.Viewport.Width - leftOffset - rightOffset;
        double effectiveViewportHeight = _map.Navigator.Viewport.Height - topOffset - bottomOffset;
        double horizontalOffsetInPixels = (leftOffset - rightOffset) / 2.0;
        double verticalOffsetInPixels = (topOffset - bottomOffset) / 2.0;

        double resolution = 0;
        double horizontalWorldOffset = 0;
        double verticalWorldOffset = 0;

        if (CurrentCountry.Code == "RU")
        {
            resolution = 7000;
            horizontalWorldOffset = horizontalOffsetInPixels * resolution;
            verticalWorldOffset = verticalOffsetInPixels * resolution;
        }
        else
        {
            GeometryFeature? countryFeature = _countriesLayer
                .GetFeatures(_mapBounds, _map.Navigator.Viewport.Resolution)
                .OfType<GeometryFeature>()
                .FirstOrDefault(f => f.Geometry != null && f.Geometry.Contains(countryPoint.ToPoint()));

            MRect boundingBox = countryFeature?.Geometry?.GetBoundingBox()
                             ?? new MRect(countryPoint.X, countryPoint.Y, countryPoint.X, countryPoint.Y);

            resolution = CalculateResolution(boundingBox, effectiveViewportWidth, effectiveViewportHeight, 1.1);

            horizontalWorldOffset = (boundingBox.Width / resolution <= effectiveViewportWidth)
                ? horizontalOffsetInPixels * resolution
                : 0;
            verticalWorldOffset = (boundingBox.Height / resolution <= effectiveViewportHeight)
                ? verticalOffsetInPixels * resolution
                : 0;
        }

        CenterMap(countryPoint, resolution, horizontalWorldOffset, verticalWorldOffset, animationDuration);
    }

    private void CenterMap(MPoint countryPoint, double resolution, double horizontalWorldOffset, double verticalWorldOffset, long animationDuration)
    {
        MPoint newCenter = new(countryPoint.X - horizontalWorldOffset, countryPoint.Y + verticalWorldOffset);
        _map.Navigator.CenterOnAndZoomTo(newCenter, resolution, animationDuration);
    }

    private List<IFeature> GetAllPins()
    {
        return GetPinFeature(_mapBounds).ToList();
    }

    private void StartFadeInPinAnimation()
    {
        StartFadePinAnimation(0, 1);
    }

    private void StartFadeOutPinAnimation()
    {
        StartFadePinAnimation(1, 0);
    }

    private void StartFadePinAnimation(double start, double end)
    {
        if (_pinsLayer == null)
        {
            return;
        }

        AnimationEntry<double> fadeAnimation = new(
            start,
            end,
            animationStart: 0,
            animationEnd: 1,
            Easing.Linear,
            repeat: false,
            tick: (_, e, v) =>
            {
                _pinsLayer.Opacity = (double)e.Start + ((double)e.End - (double)e.Start) * e.Easing.Ease(v);

                return new(_pinsLayer.Opacity, true);
            },
            final: (_, e) =>
            {
                _pinsLayer.Opacity = (double)e.End;
                _map.RefreshGraphics();

                return new(_pinsLayer.Opacity, false);
            });

        Animation.Start(fadeAnimation, 500);

        _pinFadeAnimation = fadeAnimation;
    }

    private void StartPinScaleAnimations(IFeature feature, PinAnimationType? animationType)
    {
        if (animationType == null)
        {
            return;
        }

        if (feature.Styles.FirstOrDefault() is not AnimatedCircleStyle style)
        {
            return;
        }

        List<AnimationEntry<AnimatedCircleStyle>> animations = CreatePinAnimations(feature, style, animationType.Value);

        foreach (AnimationEntry<AnimatedCircleStyle> animation in animations)
        {
            Animation.Start(animation, PINS_LAYER_ANIMATION_DURATION_IN_MS);
        }

        _activeAnimations.AddOrUpdate(style, _ => animations, (_, _) => animations);
    }

    private void InvalidateConnectPhrase()
    {
        _countryCallout.ConnectPhrase = ConnectPhrase;
    }

    private void OverrideCursor(InputCursor? cursor)
    {
        if (ProtectedCursor == cursor)
        {
            return;
        }

        ProtectedCursor = cursor;
    }
}