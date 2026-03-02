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

using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.UI.Overlays.Selection.Contracts;

namespace ProtonVPN.Client.UI.Overlays.Selection;

public partial class AppSelectorOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>, IAppSelector
{
    private readonly IMainWindowActivator _mainWindowActivator;

    private List<SelectableTunnelingApp> _originalApps = [];

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public string RemoveTooltip => Localizer.Get("Common_Actions_Remove");

    public SmartNotifyObservableCollection<SelectableTunnelingApp> Apps { get; } = [];

    public bool HasApps => Apps.Count > 0;

    public bool HasChanges => !AreAppsEqual(_originalApps, Apps);

    public AppSelectorOverlayViewModel(
        IMainWindowActivator mainWindowActivator,
        IMainWindowOverlayActivator overlayActivator,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
        _mainWindowActivator = mainWindowActivator;

        Apps.CollectionChanged += OnAppsCollectionChanged;
        Apps.ItemPropertyChanged += OnAppsItemPropertyChanged;
    }

    public async Task<List<SelectableTunnelingApp>?> SelectAsync(List<SelectableTunnelingApp> apps)
    {
        _originalApps = apps.Select(a => a.Clone()).ToList();

        Apps.Reset(apps);

        ContentDialogResult result = await InvokeAsync();
        return result switch
        {
            ContentDialogResult.Primary => Apps.ToList(),
            _ => null
        };
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(RemoveTooltip));
    }

    private static bool IsSameAppPath(string filePathA, string filePathB)
    {
        return string.Equals(filePathA, filePathB, StringComparison.OrdinalIgnoreCase);
    }

    private void OnAppsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasApps));
        OnPropertyChanged(nameof(HasChanges));
    }

    private void OnAppsItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasChanges));
    }

    [RelayCommand]
    private async Task AddAppAsync()
    {
        if (_mainWindowActivator.Window == null)
        {
            return;
        }

        string filePath = await _mainWindowActivator.Window.PickSingleFileAsync(Localizer.Get("Settings_Connection_SplitTunneling_Apps_FilesFilterName"), [ExternalApp.EXE_FILE_EXTENSION]);

        TunnelingApp? app = await TunnelingApp.TryCreateAsync(filePath);
        if (app == null)
        {
            return;
        }

        SelectableTunnelingApp? existingApp = Apps.FirstOrDefault(a => IsSameAppPath(a.Value.AppPath, app.AppPath) || a.Value.AlternateAppPaths.Any(alt => IsSameAppPath(alt, app.AppPath)));
        if (existingApp != null)
        {
            existingApp.IsSelected = true;
        }
        else
        {
            Apps.Add(new SelectableTunnelingApp(app));
        }
    }

    [RelayCommand]
    private void RemoveApp(SelectableTunnelingApp app)
    {
        Apps.Remove(app);
    }

    private bool AreAppsEqual(List<SelectableTunnelingApp> original, IList<SelectableTunnelingApp> current)
    {
        if (original.Count != current.Count)
        {
            return false;
        }

        for (int i = 0; i < original.Count; i++)
        {
            if (original[i].Value.AppPath != current[i].Value.AppPath ||
                !original[i].Value.AlternateAppPaths.SequenceEqual(current[i].Value.AlternateAppPaths) ||
                original[i].IsSelected != current[i].IsSelected)
            {
                return false;
            }
        }

        return true;
    }
}