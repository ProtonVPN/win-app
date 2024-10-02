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

using Microsoft.UI.Xaml;

namespace ProtonVPN.Client.Legacy.UI.Announcements.Banner;

public sealed partial class AnnouncementBannerComponent
{
    public static readonly DependencyProperty IsPaneOpenedProperty =
        DependencyProperty.Register(nameof(IsPaneOpened), typeof(bool), typeof(AnnouncementBannerComponent), new PropertyMetadata(default));

    public bool IsPaneOpened
    {
        get => (bool)GetValue(IsPaneOpenedProperty);
        set => SetValue(IsPaneOpenedProperty, value);
    }

    public AnnouncementBannerViewModel ViewModel { get; }

    public AnnouncementBannerComponent()
    {
        ViewModel = App.GetService<AnnouncementBannerViewModel>();

        InitializeComponent();
    }
}