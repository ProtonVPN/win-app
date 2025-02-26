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
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Core.Bases.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, ILanguageAware
{
    public IViewModelHelper ViewModelHelper { get; }

    public IUIThreadDispatcher UIThreadDispatcher { get; }

    public ILocalizationProvider Localizer { get; }

    protected ILogger Logger { get; }

    protected IIssueReporter IssueReporter { get; }

    protected ViewModelBase(IViewModelHelper viewModelHelper)
    {
        ViewModelHelper = viewModelHelper;
        UIThreadDispatcher = viewModelHelper.UIThreadDispatcher;
        Localizer = viewModelHelper.Localizer;
        Logger = viewModelHelper.Logger;
        IssueReporter = viewModelHelper.IssueReporter;
    }

    public void Receive(LanguageChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(Localizer));
            OnLanguageChanged();
        });
    }

    protected virtual void OnLanguageChanged()
    { }

    protected void InvalidateAllProperties()
    {
        OnPropertyChanged(string.Empty);
    }

    protected void ExecuteOnUIThread(Action callback,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        UIThreadDispatcher.TryEnqueue(callback);
    }
}