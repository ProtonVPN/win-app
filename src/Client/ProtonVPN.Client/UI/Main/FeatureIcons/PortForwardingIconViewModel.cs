/*
 * Copyright (c) 2024 Proton AG
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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.Helpers;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.FeatureIcons;

public class PortForwardingIconViewModel : FeatureIconViewModelBase
{
    private readonly ISettings _settings;
    private readonly IApplicationThemeSelector _themeSelector;

    public PortForwardingIconViewModel(
        IConnectionManager connectionManager,
        ISettings settings,
        IApplicationThemeSelector themeSelector,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) : base(connectionManager, localizer, logger, issueReporter)
    {
        _settings = settings;
        _themeSelector = themeSelector;
    }

    protected override bool IsFeatureEnabled => _settings.IsPortForwardingEnabled;

    protected override ImageSource GetImageSource()
    {
        return ResourceHelper.GetIllustration(
            _settings.IsPortForwardingEnabled
                ? "PortForwardingOnIllustrationSource"
                : "PortForwardingOffIllustrationSource",
            _themeSelector.GetTheme());
    }

    protected override IEnumerable<string> GetSettingsChangedForIconUpdate()
    {
        yield return nameof(ISettings.IsPortForwardingEnabled);
    }
}