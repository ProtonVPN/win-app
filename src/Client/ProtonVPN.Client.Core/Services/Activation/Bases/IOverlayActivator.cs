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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Core.Bases.ViewModels;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public interface IOverlayActivator
{
    bool HasActiveOverlay { get; }

    event EventHandler? OverlayChanged;

    OverlayViewModelBase? GetCurrentOverlayContext();

    Task<ContentDialogResult> ShowMessageAsync(MessageDialogParameters parameters);

    Task<ContentDialogResult> ShowLoadingMessageAsync(MessageDialogParameters parameters, Task loadingTask);

    Task<ContentDialogResult> ShowOverlayAsync(OverlayViewModelBase overlayViewModel, object? parameter = null);

    void CloseCurrentOverlay();
}