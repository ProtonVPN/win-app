﻿/*
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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;

namespace ProtonVPN.Client.UI.Main.Widgets.Bases;

public abstract partial class WidgetViewModelBase : ViewModelBase, IWidget
{
    public abstract int SortIndex { get; }

    public abstract string Header { get; }

    public abstract IconElement Icon { get; }

    public virtual bool IsAvailable => true;

    protected WidgetViewModelBase(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizer, logger, issueReporter)
    { }

    [RelayCommand]
    public abstract Task<bool> InvokeAsync();
}