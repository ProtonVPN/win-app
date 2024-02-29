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

using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.Overlays;

public class SmartRoutingOverlayViewModel : OverlayViewModelBase
{
    private readonly IUrls _urls;

    public string LearnMoreUrl => _urls.SmartRoutingLearnMore;

    public SmartRoutingOverlayViewModel(
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        IUrls urls,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider,
            logger,
            issueReporter,
            overlayActivator)
    {
        _urls = urls;
    }
}