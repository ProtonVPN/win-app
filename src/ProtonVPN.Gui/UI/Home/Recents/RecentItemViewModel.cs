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

using Windows.ApplicationModel.Resources;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Recents.Contracts;
using ProtonVPN.Gui.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProtonVPN.Gui.UI.Home.Recents;

public partial class RecentItemViewModel : ViewModelBase
{
    private readonly IRecentConnection _recentConnection;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    private string? _entryCountry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private string? _exitCountry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    private bool _isSecureCore;

    [ObservableProperty]
    private bool _isPinned;

    [ObservableProperty]
    private bool _isActiveConnection;

    public RecentItemViewModel(IRecentConnection recentConnection)
    {
        _recentConnection = recentConnection;

        _entryCountry = _recentConnection.EntryCountryCode;
        _exitCountry = _recentConnection.ExitCountryCode;
        _isPinned = _recentConnection.IsPinned;
        _isSecureCore = !_entryCountry.IsNullOrEmpty() && !_exitCountry.IsNullOrEmpty();
    }

    public string Title => !ExitCountry.IsNullOrEmpty()
        ? Localizer.Get($"Country_val_{ExitCountry}")
        : Localizer.Get("Country_Fastest");

    public string? Subtitle => IsSecureCore
        ? Localizer.GetFormat("Connection_Via_SecureCore", Localizer.Get($"Country_val_{EntryCountry}"))
        : _recentConnection.City ?? _recentConnection.Server;

    public bool HasSubtitle => !Subtitle.IsNullOrEmpty();

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(HasSubtitle));
    }
}