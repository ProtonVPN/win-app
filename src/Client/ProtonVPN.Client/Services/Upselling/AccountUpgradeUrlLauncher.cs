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

using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.Services.Upselling;

public class AccountUpgradeUrlLauncher : IAccountUpgradeUrlLauncher,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IUpsellUpgradeAttemptStatisticalEventSender _upsellUpgradeAttemptStatisticalEventSender;
    private readonly IUpsellSuccessStatisticalEventSender _upsellSuccessStatisticalEventSender;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IWebAuthenticator _webAuthenticator;

    private ModalSource? _currentAttemptModalSource;
    private string? _currentAttemptReference;

    public AccountUpgradeUrlLauncher(
        IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender,
        IUpsellSuccessStatisticalEventSender upsellSuccessStatisticalEventSender,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator)
    {
        _upsellUpgradeAttemptStatisticalEventSender = upsellUpgradeAttemptStatisticalEventSender;
        _upsellSuccessStatisticalEventSender = upsellSuccessStatisticalEventSender;
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;
    }

    public async Task OpenAsync(ModalSource modalSource, string? reference = null)
    {
        string url = await _webAuthenticator.GetUpgradeAccountUrlAsync(modalSource, reference);

        Open(url, modalSource, reference);
    }

    public void Open(string url, ModalSource modalSource, string? reference = null)
    {
        try
        {
            _upsellUpgradeAttemptStatisticalEventSender.Send(modalSource, reference);

            _urlsBrowser.BrowseTo(url);
        }
        finally
        {
            SetAttempt(modalSource, reference);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        try
        {
            if (_currentAttemptModalSource.HasValue && message.HasChanged() && !message.IsDowngrade())
            {
                _upsellSuccessStatisticalEventSender.Send(_currentAttemptModalSource.Value, message.OldPlan, message.NewPlan, _currentAttemptReference);
            }
        }
        finally
        {
            ResetAttempt();
        }
    }

    private void SetAttempt(ModalSource modalSource, string? reference)
    {
        _currentAttemptModalSource = modalSource;
        _currentAttemptReference = reference;
    }

    private void ResetAttempt()
    {
        _currentAttemptModalSource = null;
        _currentAttemptReference = null;
    }
}
