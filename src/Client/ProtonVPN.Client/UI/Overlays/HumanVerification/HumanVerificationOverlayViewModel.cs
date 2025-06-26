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

using Microsoft.UI.Xaml;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Services.Verification.Messages;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.UI.Overlays.HumanVerification;

public partial class HumanVerificationOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<RequestTokenMessage>,
    IEventMessageReceiver<ThemeChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IApplicationThemeSelector _themeSelector;
    private readonly IApiHostProvider _apiHostProvider;

    private string _token = string.Empty;

    public string Url => GetCaptchaUrl();

    public HumanVerificationOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        IEventMessageSender eventMessageSender,
        IApplicationThemeSelector themeSelector,
        IApiHostProvider apiHostProvider,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
        _themeSelector = themeSelector;
        _apiHostProvider = apiHostProvider;
    }

    public void Receive(RequestTokenMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            _token = message.Token;
            OnPropertyChanged(nameof(Url));
        });
    }

    public void Receive(ThemeChangedMessage message)
    {
        if (!IsActive)
        {
            return;
        }

        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(Url));
        });
    }

    public void TriggerVerificationTokenMessage(string token)
    {
        _eventMessageSender.Send(new ResponseTokenMessage(token));

        Close();
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Logger.Info<AppLog>($"Human verification triggered (url: {Url})");
    }

    private string GetCaptchaUrl()
    {
        bool isDarkTheme = _themeSelector.GetTheme() == ElementTheme.Dark;
        string relativeUri = $"core/v4/captcha?Token={_token}{(isDarkTheme ? "&Dark=1" : string.Empty)}";
        Uri requestUri = new(_apiHostProvider.GetBaseUri(), relativeUri);
        return requestUri.AbsoluteUri;
    }
}