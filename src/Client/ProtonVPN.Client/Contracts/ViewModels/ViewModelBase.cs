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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, ILanguageAware
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public ILocalizationProvider Localizer { get; }

    protected ViewModelBase(ILocalizationProvider localizationProvider)
    {
        Localizer = localizationProvider;
    }

    public void Receive(LanguageChangedMessage message)
    {
        OnPropertyChanged(nameof(Localizer));
        OnLanguageChanged();
    }

    protected virtual void OnLanguageChanged()
    {
        // Override this method to invalidate any localized strings set up in your viewmodel
    }

    protected void InvalidateAllProperties()
    {
        OnPropertyChanged(string.Empty);
    }

    protected void ExecuteOnUIThread(Action callback)
    {
        _dispatcherQueue.TryEnqueue(() => callback());
    }
}