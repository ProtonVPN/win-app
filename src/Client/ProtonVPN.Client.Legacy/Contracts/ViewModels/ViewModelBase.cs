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

using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Legacy.Contracts.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, ILanguageAware
{
    private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public ILocalizationProvider Localizer { get; }

    protected ILogger Logger { get; }
    protected IIssueReporter IssueReporter { get; }

    protected ViewModelBase(ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
    {
        Localizer = localizationProvider;
        Logger = logger;
        IssueReporter = issueReporter;
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
    {
        // Override this method to invalidate any localized strings set up in your viewmodel
    }

    protected void InvalidateAllProperties()
    {
        OnPropertyChanged(string.Empty);
    }

    protected void ExecuteOnUIThread(Action callback,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        _dispatcherQueue.TryEnqueue(() => ExecuteSafely(callback, sourceFilePath, sourceMemberName, sourceLineNumber));
    }

    private void ExecuteSafely(Action callback, string sourceFilePath, string sourceMemberName, int sourceLineNumber)
    {
        try
        {
            callback();
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>($"Exception handled by {nameof(ViewModelBase)} {nameof(ExecuteSafely)}.",
                ex, sourceFilePath, sourceMemberName, sourceLineNumber);
            IssueReporter.CaptureError(ex, sourceFilePath, sourceMemberName, sourceLineNumber);
        }
    }
}