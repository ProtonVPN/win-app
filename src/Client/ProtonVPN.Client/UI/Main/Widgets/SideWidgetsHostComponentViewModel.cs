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

using System.Collections.ObjectModel;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;

namespace ProtonVPN.Client.UI.Main.Widgets;

public partial class SideWidgetsHostComponentViewModel : ViewModelBase
{
    public ObservableCollection<ISideHeaderWidget> HeaderWidgets { get; }

    public ObservableCollection<ISideFooterWidget> FooterWidgets { get; }

    public bool HasHeaderAndFooterWidgets => HeaderWidgets.Count > 0 && FooterWidgets.Count > 0;

    public SideWidgetsHostComponentViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IEnumerable<ISideHeaderWidget> headerWidgets,
        IEnumerable<ISideFooterWidget> footerWidgets)
        : base(localizer, logger, issueReporter)
    {
        HeaderWidgets = new(headerWidgets.OrderBy(p => p.SortIndex));
        FooterWidgets = new(footerWidgets.OrderBy(p => p.SortIndex));
    }
}