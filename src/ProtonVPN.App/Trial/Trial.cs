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
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Window.Popups;
using ProtonVPN.Modals.Welcome;
using ProtonVPN.Windows.Popups.Trials;
using ProtonVPN.Windows.Popups.Upsell;

namespace ProtonVPN.Trial
{
    public sealed class Trial : ITrialDurationAware, IVpnStateAware, IVpnPlanAware
    {
        private readonly Random random = new();
        private readonly IAppSettings _appSettings;
        private readonly UserAuth _userAuth;
        private readonly IUserStorage _userStorage;
        private readonly IPopupWindows _popupWindows;
        private readonly IModals _modals;
        
        public event EventHandler<PlanStatus> StateChanged;

        public Trial(
            IAppSettings appSettings,
            UserAuth userAuth,
            IUserStorage userStorage,
            IPopupWindows popupWindows, 
            IModals modals)
        {
            _appSettings = appSettings;
            _userAuth = userAuth;
            _userStorage = userStorage;
            _popupWindows = popupWindows;
            _modals = modals;
        }

        public async Task Load()
        {
            User user = _userStorage.User();
            if (_appSettings.TrialExpirationTime == 0)
            {
                if (user.TrialStatus().Equals(PlanStatus.TrialNotStarted))
                {
                    InvokeStateChange(user.TrialStatus());
                }

                if (user.TrialStatus().Equals(PlanStatus.TrialStarted))
                {
                    await Start();
                }

                if (user.VpnPlan.Equals("free") && user.TrialStatus().Equals(PlanStatus.Free))
                {
                    InvokeStateChange(PlanStatus.Free);
                }
            }
            else if (_appSettings.TrialExpirationTime > 0)
            {
                if (user.TrialStatus().Equals(PlanStatus.TrialStarted))
                {
                    await Start();
                }

                InvokeStateChange(user.TrialStatus());
            }

            HandleModals();
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (!e.State.Status.Equals(VpnStatus.Connected))
            {
                return;
            }

            User user = _userStorage.User();
            if (user.TrialStatus().Equals(PlanStatus.TrialNotStarted))
            {
                await Start();
            }
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
            {
                ShowTrialAboutToExpireModal();
            }
        }

        private void HandleModals()
        {
            User user = _userStorage.User();

            if (WelcomeModalHasToBeShown())
            {
                ShowWelcomeModal();
            }
            else if (TrialEndModalHasToBeShown())
            {
                ShowTrialEndModal();
            }
            else if (!user.Paid() && !_userStorage.User().IsDelinquent())
            {
                ShowEnjoyModal();
            }
        }

        private void ShowEnjoyModal()
        {
            int randomNumber = random.Next(0, 100);
            if (randomNumber >= 15)
            {
                return;
            }

            _popupWindows.Show<EnjoyingUpsellPopupViewModel>();
        }

        private void InvokeStateChange(PlanStatus status)
        {
            StateChanged?.Invoke(this, status);
        }

        private bool TrialIsAboutToExpire()
        {
            User user = _userStorage.User();
            return user.TrialExpirationTimeInSeconds() <= 60 * 60 * 48;
        }

        private void ShowTrialAboutToExpireModal()
        {
            if (_appSettings.AboutToExpireModalShown || !_appSettings.WelcomeModalShown)
            {
                return;
            }

            _popupWindows.Show<TrialPopupViewModel>();
            _appSettings.AboutToExpireModalShown = true;
        }

        private void ShowWelcomeModal()
        {
            User user = _userStorage.User();
            if (user.VpnPlan.Equals("trial"))
            {
                _modals.Show<TrialWelcomeModalViewModel>();
            }
            else
            {
                _modals.Show<NonTrialWelcomeModalViewModel>();
            }

            _appSettings.WelcomeModalShown = true;
        }

        private bool TrialEndModalHasToBeShown()
        {
            User user = _userStorage.User();
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
            _popupWindows.Show<TrialPopupViewModel>();
            _appSettings.ExpiredModalShown = true;
        }

        public Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            User user = _userStorage.User();
            InvokeStateChange(user.TrialStatus());

            return Task.CompletedTask;
        }
    }
}