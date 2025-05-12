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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Core.Bases.Helpers;

public class ViewModelHelper : IViewModelHelper
{
    public IUIThreadDispatcher UIThreadDispatcher { get; }

    public ILocalizationProvider Localizer { get; }

    public ILogger Logger { get; }

    public IIssueReporter IssueReporter { get; }

    public ViewModelHelper(
        IUIThreadDispatcher uiThreadDispatcher,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
    {
        UIThreadDispatcher = uiThreadDispatcher;
        Localizer = localizationProvider;
        Logger = logger;
        IssueReporter = issueReporter;
    }
}