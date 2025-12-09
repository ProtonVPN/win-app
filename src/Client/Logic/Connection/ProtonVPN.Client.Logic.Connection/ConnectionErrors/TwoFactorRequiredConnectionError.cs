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
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Logic.Connection.ConnectionErrors;

public class TwoFactorRequiredConnectionError : ConnectionErrorBase, 
    IEventMessageReceiver<FeatureFlagsChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly ISettings _settings;

    private string _u2fGatewayPortalUrl = string.Empty;

    public override Severity Severity => Severity.Warning;

    public override string Title => Localizer.Get("Connection_Error_TwoFactorRequired_Title");

    public override string Message => Localizer.Get("Connection_Error_TwoFactorRequired_Description");

    public override string ActionLabel => _u2fGatewayPortalUrl.IsValidUrl() ? Localizer.Get("Login_TwoFactorForm_Authenticate") : string.Empty;

    public override bool IsToCloseErrorOnDisconnect => true;

    public TwoFactorRequiredConnectionError(
        ILocalizationProvider localizer,
        IFeatureFlagsObserver featureFlagsObserver,
        IUrlsBrowser urlsBrowser,
        IUIThreadDispatcher uiThreadDispatcher,
        ISettings settings)
        : base(localizer)
    {
        _featureFlagsObserver = featureFlagsObserver;
        _urlsBrowser = urlsBrowser;
        _uiThreadDispatcher = uiThreadDispatcher;
        _settings = settings;

        InvalidatePortalUrl();
    }

    public override Task ExecuteActionAsync()
    {
        if (_u2fGatewayPortalUrl.IsValidUrl())
        {
            _urlsBrowser.BrowseTo(_u2fGatewayPortalUrl);
        }

        return Task.CompletedTask;
    }

    private void InvalidatePortalUrl()
    {
        string baseUrl = _featureFlagsObserver.U2FGatewayPortalUrl;
        if (!baseUrl.IsValidUrl())
        {
            _u2fGatewayPortalUrl = string.Empty;
            return;
        }

        UriBuilder uriBuilder = new(baseUrl)
        {
            Query = $"email={_settings.UserEmail}",
        };

        string finalUrl = uriBuilder.Uri.ToString();

        _u2fGatewayPortalUrl = finalUrl.IsValidUrl()
            ? finalUrl
            : baseUrl;
    }

    public void Receive(FeatureFlagsChangedMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(InvalidatePortalUrl);
    }

    public void Receive(LoggedInMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(InvalidatePortalUrl);
    }
}