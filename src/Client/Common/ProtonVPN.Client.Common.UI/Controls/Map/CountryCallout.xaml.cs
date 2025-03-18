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

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.Common.UI.Controls.Map;

public sealed partial class CountryCallout : INotifyPropertyChanged
{
    private const int HORIZONTAL_PADDING = 6;
    private const int VERTICAL_PADDING = 12;
    private const int CORNER_RADIUS = 8;
    private const int ARROW_WIDTH = 16;
    private const double ARROW_HEIGHT = 7.5;

    private const string VISUAL_STATE_GROUP_VISIBILITY = "VisibilityStates";
    private const string VISUAL_STATE_VISIBLE = "Visible";
    private const string VISUAL_STATE_COLLAPSED = "Collapsed";

    private const string VISUAL_STATE_UNDER_MAINTENANCE = "UnderMaintenance";
    private const string VISUAL_STATE_NOT_UNDER_MAINTENANCE = "NotUnderMaintenance";

    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DependencyProperty CountryProperty = DependencyProperty.Register(
        nameof(Country),
        typeof(Country),
        typeof(CountryCallout),
        new PropertyMetadata(null, OnCountryChanged));

    public static readonly DependencyProperty ConnectPhraseProperty = DependencyProperty.Register(
        nameof(ConnectPhrase),
        typeof(string),
        typeof(CountryCallout),
        new PropertyMetadata(default, OnConnectPhraseChanged));

    public Country? Country
    {
        get => (Country?)GetValue(CountryProperty);
        set => SetValue(CountryProperty, value);
    }

    public string ConnectPhrase
    {
        get => (string)GetValue(ConnectPhraseProperty);
        set => SetValue(ConnectPhraseProperty, value);
    }

    public string Location => string.Format(ConnectPhrase, Country?.Name);

    private bool _isUnloaded;
    private Country? _lastCountry;

    public CountryCallout()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isUnloaded = true;
    }

    public void Show(Country country)
    {
        if (_isUnloaded)
        {
            return;
        }

        Country = country;
        Visibility = Visibility.Visible;

        VisualStateManager.GoToState(this, VISUAL_STATE_VISIBLE, true);
        VisualStateManager.GoToState(this, country.IsUnderMaintenance ? VISUAL_STATE_UNDER_MAINTENANCE : VISUAL_STATE_NOT_UNDER_MAINTENANCE, true);
    }

    public void Hide()
    {
        if (_isUnloaded)
        {
            return;
        }

        VisualStateManager.GoToState(this, VISUAL_STATE_COLLAPSED, true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isUnloaded = false;

        LayoutUpdated += OnContentPanelLayoutUpdated;

        IList<VisualStateGroup> groups = VisualStateManager.GetVisualStateGroups(RootGrid);
        VisualStateGroup? visibilityGroup = groups.FirstOrDefault(g => g.Name == VISUAL_STATE_GROUP_VISIBILITY);

        if (visibilityGroup != null)
        {
            visibilityGroup.CurrentStateChanged += OnCurrentVisibilityStateChanged;
        }
    }

    private void OnCurrentVisibilityStateChanged(object sender, VisualStateChangedEventArgs e)
    {
        if (e.NewState?.Name == VISUAL_STATE_COLLAPSED)
        {
            Visibility = Visibility.Collapsed;
        }
    }

    private static void OnConnectPhraseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CountryCallout control)
        {
            return;
        }

        control.InvalidateCountryLabel();
    }

    private static void OnCountryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CountryCallout control)
        {
            return;
        }

        control.InvalidateCountryLabel();
    }

    private void InvalidateCountryLabel()
    {
        OnPropertyChanged(nameof(Location));
    }

    private void OnContentPanelLayoutUpdated(object? sender, object e)
    {
        if (Country is not null && (_lastCountry is null || _lastCountry != Country) && ContentPanel.ActualWidth > 0 && ContentPanel.ActualHeight > 0)
        {
            _lastCountry = Country;

            double width = ContentPanel.ActualWidth + HORIZONTAL_PADDING * 2;
            double height = ContentPanel.ActualHeight + VERTICAL_PADDING * 2;

            string path = BuildBubblePath(width, height);
            BubblePath.Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), path);
        }
    }

    public string BuildBubblePath(double width, double height)
    {
        double arrowHalf = ARROW_WIDTH / 2.0;
        double arrowCenterX = width / 2.0;
        double bottomRectY = height - ARROW_HEIGHT;

        return string.Format(CultureInfo.InvariantCulture,
            "M 0,{0} " +
            "A {0},{0} 0 0 1 {0},0 " +                // Top-left corner arc
            "H {1} " +                                // Top edge line
            "A {0},{0} 0 0 1 {2},{0} " +              // Top-right corner arc
            "V {3} " +                                // Right edge down
            "A {0},{0} 0 0 1 {1},{4} " +              // Bottom-right corner arc
            "L {5},{4} " +                            // Arrow start (right shoulder)
            "L {6},{7} " +                            // Arrow tip
            "L {8},{4} " +                            // Arrow start (left shoulder)
            "L {0},{4} " +                            // Bottom edge to left corner arc start
            "A {0},{0} 0 0 1 0,{3} " +                // Bottom-left corner arc
            "V {0} " +                                // Left edge up
            "Z",                                      // Close
        CORNER_RADIUS,                   // {0}
        (width - CORNER_RADIUS),         // {1}
        width,                           // {2}
        (bottomRectY - CORNER_RADIUS),   // {3}
        bottomRectY,                     // {4}
        arrowCenterX + arrowHalf,        // {5}
        arrowCenterX,                    // {6}
        height,                          // {7}
        arrowCenterX - arrowHalf);       // {8}
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}