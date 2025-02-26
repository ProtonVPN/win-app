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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Core.Services.Activation.Bases;

namespace ProtonVPN.Client.Core.Bases.ViewModels;

public abstract partial class OverlayViewModelBase : ActivatableViewModelBase, IOverlayActivationAware
{
    protected OverlayViewModelBase(IViewModelHelper viewModelHelper) : base(viewModelHelper)
    {
    }

    public virtual void OnShow(object? parameter)
    { }
}

public abstract partial class OverlayViewModelBase<TOverlayActivator> : OverlayViewModelBase, IOverlayActivatorAware
    where TOverlayActivator : IOverlayActivator
{
    protected readonly TOverlayActivator OverlayActivator;

    public virtual bool CanClose => OverlayActivator.GetCurrentOverlayContext() == this;

    protected OverlayViewModelBase(
        TOverlayActivator overlayActivator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        OverlayActivator = overlayActivator;
        OverlayActivator.OverlayChanged += OnOverlayChanged;
    }

    public Task<ContentDialogResult> InvokeAsync(object? parameter = null)
    {
        return OverlayActivator.ShowOverlayAsync(this, parameter);
    }

    public void Close()
    {
        if (CanClose)
        {
            OverlayActivator.CloseCurrentOverlay();
        }
    }

    [RelayCommand]
    private Task<ContentDialogResult> ShowOverlayAsync()
    {
        return InvokeAsync();
    }

    [RelayCommand(CanExecute = nameof(CanClose))]
    private void CloseOverlay()
    {
        Close();
    }

    private void OnOverlayChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanClose));
        CloseOverlayCommand.NotifyCanExecuteChanged();
    }
}