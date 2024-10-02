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

using Microsoft.UI.Xaml;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Services.Verification.Messages;

namespace ProtonVPN.Client.UI.Overlays.HumanVerification;

public partial class HumanVerificationOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<RequestTokenMessage>,
    IEventMessageReceiver<ThemeChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IApplicationThemeSelector _themeSelector;
    private readonly IApiHostProvider _apiHostProvider;

    private string _token = string.Empty;

    public bool IsDarkTheme => _themeSelector.GetTheme() == ElementTheme.Dark;

    public string Url => GetCaptchaUrl();

    public HumanVerificationOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IEventMessageSender eventMessageSender,
        IApplicationThemeSelector themeSelector,
        IApiHostProvider apiHostProvider)
        : base(overlayActivator, localizer, logger, issueReporter)
    {
        _eventMessageSender = eventMessageSender;
        _themeSelector = themeSelector;
        _apiHostProvider = apiHostProvider;
    }

    public void Receive(RequestTokenMessage message)
    {
        _token = message.Token;
        OnPropertyChanged(nameof(Url));
    }

    public void Receive(ThemeChangedMessage message)
    {
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(Url));
    }

    public void TriggerVerificationTokenMessage(string token)
    {
        _eventMessageSender.Send(new ResponseTokenMessage(token));

        Close();
    }

    private string GetCaptchaUrl()
    {
        string relativeUri = $"core/v4/captcha?Token={_token}{(IsDarkTheme ? "&Dark=1" : string.Empty)}";
        Uri requestUri = new(_apiHostProvider.GetBaseUri(), relativeUri);
        return requestUri.AbsoluteUri;
    }
}