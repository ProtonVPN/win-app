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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.ViewModels;

namespace ProtonVPN.Client.Models.Activation;

public interface IOverlayActivator
{
    Task<ContentDialogResult> ShowMessageAsync(MessageDialogParameters parameters, Window? rootWindow = null);

    Task<ContentDialogResult> ShowLoadingMessageAsync(MessageDialogParameters parameters, Task task, Window? rootWindow = null);

    Task ShowOverlayAsync<TOverlayViewModel>(Window? rootWindow = null)
        where TOverlayViewModel : OverlayViewModelBase;

    Task ShowOverlayAsync(string overlayKey, Window? rootWindow = null);

    void CloseOverlay<TOverlayViewModel>(Window? rootWindow = null)
        where TOverlayViewModel : OverlayViewModelBase;

    void CloseOverlay(string overlayKey, Window? rootWindow = null);

    void CloseAllOverlays(Window? rootWindow = null);
}