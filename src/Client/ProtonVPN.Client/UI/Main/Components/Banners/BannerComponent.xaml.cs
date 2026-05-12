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

using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ProtonVPN.Client.UI.Main.Components.Banners;

public sealed partial class BannerComponent
{
    public static readonly DependencyProperty IsInfoBannerVisibleProperty = DependencyProperty.Register(
        nameof(IsInfoBannerVisible), typeof(bool), typeof(BannerComponent), new PropertyMetadata(false));

    public static readonly DependencyProperty InfoBannerDescriptionProperty = DependencyProperty.Register(
        nameof(InfoBannerDescription), typeof(string), typeof(BannerComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty InfoBannerIllustrationSourceProperty = DependencyProperty.Register(
        nameof(InfoBannerIllustrationSource), typeof(ImageSource), typeof(BannerComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty InfoBannerDismissButtonTextProperty = DependencyProperty.Register(
        nameof(InfoBannerDismissButtonText), typeof(string), typeof(BannerComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty InfoBannerDismissCommandProperty = DependencyProperty.Register(
        nameof(InfoBannerDismissCommand), typeof(ICommand), typeof(BannerComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty IsUpsellBannerVisibleProperty = DependencyProperty.Register(
        nameof(IsUpsellBannerVisible), typeof(bool), typeof(BannerComponent), new PropertyMetadata(false));

    public static readonly DependencyProperty UpsellBannerTitleProperty = DependencyProperty.Register(
        nameof(UpsellBannerTitle), typeof(string), typeof(BannerComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty UpsellBannerIllustrationSourceProperty = DependencyProperty.Register(
        nameof(UpsellBannerIllustrationSource), typeof(ImageSource), typeof(BannerComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty UpsellBannerCommandProperty = DependencyProperty.Register(
        nameof(UpsellBannerCommand), typeof(ICommand), typeof(BannerComponent), new PropertyMetadata(null));

    public bool IsInfoBannerVisible
    {
        get => (bool)GetValue(IsInfoBannerVisibleProperty);
        set => SetValue(IsInfoBannerVisibleProperty, value);
    }

    public string InfoBannerDescription
    {
        get => (string)GetValue(InfoBannerDescriptionProperty);
        set => SetValue(InfoBannerDescriptionProperty, value);
    }

    public ImageSource InfoBannerIllustrationSource
    {
        get => (ImageSource)GetValue(InfoBannerIllustrationSourceProperty);
        set => SetValue(InfoBannerIllustrationSourceProperty, value);
    }

    public string InfoBannerDismissButtonText
    {
        get => (string)GetValue(InfoBannerDismissButtonTextProperty);
        set => SetValue(InfoBannerDismissButtonTextProperty, value);
    }

    public ICommand InfoBannerDismissCommand
    {
        get => (ICommand)GetValue(InfoBannerDismissCommandProperty);
        set => SetValue(InfoBannerDismissCommandProperty, value);
    }

    public bool IsUpsellBannerVisible
    {
        get => (bool)GetValue(IsUpsellBannerVisibleProperty);
        set => SetValue(IsUpsellBannerVisibleProperty, value);
    }

    public string UpsellBannerTitle
    {
        get => (string)GetValue(UpsellBannerTitleProperty);
        set => SetValue(UpsellBannerTitleProperty, value);
    }

    public ImageSource UpsellBannerIllustrationSource
    {
        get => (ImageSource)GetValue(UpsellBannerIllustrationSourceProperty);
        set => SetValue(UpsellBannerIllustrationSourceProperty, value);
    }

    public ICommand UpsellBannerCommand
    {
        get => (ICommand)GetValue(UpsellBannerCommandProperty);
        set => SetValue(UpsellBannerCommandProperty, value);
    }

    public BannerViewModel ViewModel { get; }

    public BannerComponent()
    {
        ViewModel = App.GetService<BannerViewModel>();

        InitializeComponent();
    }
}