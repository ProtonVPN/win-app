/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals.Trial;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Modals.Welcome;

namespace ProtonVPN.Trial
{
    public sealed class Trial : ITrialDurationAware, IVpnStateAware, IVpnPlanAware
    {
        private readonly IAppSettings _appSettings;
        private readonly UserAuth _userAuth;
        private readonly IUserStorage _userStorage;
        private readonly IModals _modals;

        public event EventHandler<PlanStatus> StateChanged;

        public Trial(
            IAppSettings appSettings,
            UserAuth userAuth,
            IUserStorage userStorage,
            IModals modals)
        {
            _modals = modals;
            _userStorage = userStorage;
            _userAuth = userAuth;
            _appSettings = appSettings;
        }

        public async Task Load()
        {
            var user = _userStorage.User();
            if (_appSettings.TrialExpirationTime == 0)
            {
                if (user.TrialStatus().Equals(PlanStatus.TrialNotStarted))
                    InvokeStateChange(user.TrialStatus());
                if (user.TrialStatus().Equals(PlanStatus.TrialStarted))
                    await Start();
                if (user.VpnPlan.Equals("free") && user.TrialStatus().Equals(PlanStatus.Free))
                    InvokeStateChange(PlanStatus.Free);
            }
            else if (_appSettings.TrialExpirationTime > 0)
            {
                if (user.TrialStatus().Equals(PlanStatus.TrialStarted))
                    await Start();

                InvokeStateChange(user.TrialStatus());
            }

            HandleModals();
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!e.State.Status.Equals(VpnStatus.Connected))
                return;

            var user = _userStorage.User();
            if (user.TrialStatus().Equals(PlanStatus.TrialNotStarted))
                await Start();
        }

        public async Task Start()
        {
            await _userAuth.RefreshVpnInfo(response =>
            {
                _appSettings.TrialExpirationTime = response.Vpn.ExpirationTime;
                InvokeStateChange(PlanStatus.TrialStarted);
            });
        }

        public void OnTrialSecondElapsed(TrialTickEventArgs e)
        {
            if (e.SecondsLeft <= 0)
            {
                InvokeStateChange(PlanStatus.Expired);
                _appSettings.TrialExpirationTime = 0;
                _userStorage.SetFreePlan();
                ShowTrialEndModal();
            }
            else if (TrialIsAboutToExpire())
                ShowTrialAboutToExpireModal();
        }

        private void HandleModals()
        {
            var user = _userStorage.User();

            if (WelcomeModalHasToBeShown())
                ShowWelcomeModal();
            else if (TrialEndModalHasToBeShown())
                ShowTrialEndModal();
            else if (!user.Paid())
                ShowEnjoyModal();
        }

        private void ShowEnjoyModal()
        {
            var rand = new Random();
            var randomNumber = rand.Next(0, 100);
            if (randomNumber >= 15) return;
            _modals.Show<EnjoyingUpsellModalViewModel>();
        }

        private void InvokeStateChange(PlanStatus status)
        {
            StateChanged?.Invoke(this, status);
        }

        private bool TrialIsAboutToExpire()
        {
            var user = _userStorage.User();
            return user.TrialExpirationTimeInSeconds() <= 60 * 60 * 48;
        }

        private void ShowTrialAboutToExpireModal()
        {
            if (_appSettings.AboutToExpireModalShown || !_appSettings.WelcomeModalShown)
                return;

            _modals.Show<TrialAboutToExpireModalViewModel>();
            _appSettings.AboutToExpireModalShown = true;
        }

        private void ShowWelcomeModal()
        {
            var user = _userStorage.User();
            if (user.VpnPlan.Equals("trial"))
                _modals.Show<TrialWelcomeModalViewModel>();
            else
                _modals.Show<NonTrialWelcomeModalViewModel>();

            _appSettings.WelcomeModalShown = true;
        }

        private bool TrialEndModalHasToBeShown()
        {
            var user = _userStorage.User();
            return !_appSettings.ExpiredModalShown &&
                   _appSettings.WelcomeModalShown &&
                   _appSettings.TrialExpirationTime > 0 &&
                   user.TrialStatus() == PlanStatus.Free;
        }

        private bool WelcomeModalHasToBeShown()
        {
            return !_appSettings.WelcomeModalShown;
        }

        private void ShowTrialEndModal()
        {
            _modals.Show<TrialEndModalViewModel>();
            _appSettings.ExpiredModalShown = true;
        }

        public Task OnVpnPlanChangedAsync(string plan)
        {
            var user = _userStorage.User();
            InvokeStateChange(user.TrialStatus());

            return Task.CompletedTask;
        }
    }
}
