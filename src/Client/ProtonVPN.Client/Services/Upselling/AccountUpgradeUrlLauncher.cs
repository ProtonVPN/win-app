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
    private readonly IUpsellUpgradeAttemptReporter _upsellUpgradeAttemptReporter;
    private readonly IUpsellSuccessReporter _upsellSuccessReporter;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IWebAuthenticator _webAuthenticator;

    private string? _currentAttemptUrl;
    private UpsellModalContext? _currentAttemptContext;
    private string? _currentAttemptReference;

    public AccountUpgradeUrlLauncher(
        IUpsellUpgradeAttemptReporter upsellUpgradeAttemptReporter,
        IUpsellSuccessReporter upsellSuccessReporter,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator)
    {
        _upsellUpgradeAttemptReporter = upsellUpgradeAttemptReporter;
        _upsellSuccessReporter = upsellSuccessReporter;
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;
    }

    public async Task OpenAsync(UpsellModalContext context)
    {
        string url = await _webAuthenticator.GetUpgradeAccountUrlAsync(context.Source);

        Open(url, context);
    }

    public void Open(string url, UpsellModalContext context, string? reference = null)
    {
        try
        {
            _upsellUpgradeAttemptReporter.Report(context, reference);

            _urlsBrowser.BrowseTo(url);
        }
        finally
        {
            SetAttempt(url, context, reference);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        try
        {
            if (_currentAttemptContext.HasValue && message.HasChanged() && !message.IsDowngrade())
            {
                _upsellSuccessReporter.Report(
                    _currentAttemptUrl ?? string.Empty,
                    _currentAttemptContext.Value,
                    message.OldPlan,
                    message.NewPlan,
                    _currentAttemptReference);
            }
        }
        finally
        {
            ResetAttempt();
        }
    }

    private void SetAttempt(string url, UpsellModalContext context, string? reference)
    {
        _currentAttemptUrl = url;
        _currentAttemptContext = context;
        _currentAttemptReference = reference;
    }

    private void ResetAttempt()
    {
        _currentAttemptUrl = null;
        _currentAttemptContext = null;
        _currentAttemptReference = null;
    }
}
