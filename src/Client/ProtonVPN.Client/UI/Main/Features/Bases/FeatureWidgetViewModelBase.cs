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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Widgets.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.Bases;

public abstract class FeatureWidgetViewModelBase : SideWidgetViewModelBase, ISideHeaderWidget
{
    protected readonly ISettings Settings;

    private readonly ImageIcon _featureIcon = new();

    public override int SortIndex => (int)ConnectionFeature;

    public string Status => GetFeatureStatus();

    public ConnectionFeature ConnectionFeature { get; }

    protected FeatureWidgetViewModelBase(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainViewNavigator mainViewNavigator,
        ISettings settings,
        ConnectionFeature connectionFeature)
        : base(localizer, logger, issueReporter, mainViewNavigator)
    {
        Settings = settings;

        ConnectionFeature = connectionFeature;
    }

    public override Task<bool> InvokeAsync()
    {
        return MainViewNavigator.NavigateToFeatureViewAsync(ConnectionFeature);
    }

    protected abstract string GetFeatureStatus();

    protected override void InvalidateIsSelected()
    {
    }
}