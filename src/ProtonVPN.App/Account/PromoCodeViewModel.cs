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

using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Translations;

namespace ProtonVPN.Account
{
    public class PromoCodeViewModel : Screen
    {
        private readonly IPromoCodeManager _promoCodeManager;
        private readonly IVpnInfoUpdater _vpnInfoUpdater;
        private readonly IEventAggregator _eventAggregator;

        private string _error = string.Empty;
        private string _promoCode;
        private bool _isPromoPopupVisible;
        private bool _sending;

        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public string PromoCode
        {
            get => _promoCode;
            set
            {
                Set(ref _promoCode, value);
                ApplyCouponCodeCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsPromoPopupVisible
        {
            get => _isPromoPopupVisible;
            set
            {
                Set(ref _isPromoPopupVisible, value);
                if (!value)
                {
                    Error = string.Empty;
                    PromoCode = string.Empty;
                }
            }
        }

        public bool Sending
        {
            get => _sending;
            set => Set(ref _sending, value);
        }

        public RelayCommand ApplyCouponCodeCommand { get; set; }

        public PromoCodeViewModel(IPromoCodeManager promoCodeManager, IVpnInfoUpdater vpnInfoUpdater,
            IEventAggregator eventAggregator)
        {
            _promoCodeManager = promoCodeManager;
            _vpnInfoUpdater = vpnInfoUpdater;
            _eventAggregator = eventAggregator;
            ApplyCouponCodeCommand = new RelayCommand(ApplyCouponCodeAction, AllowToApplyPromoCode);
        }

        public void ClosePopup()
        {
            IsPromoPopupVisible = false;
        }

        private async void ApplyCouponCodeAction()
        {
            if (Sending)
            {
                return;
            }

            Sending = true;
            Result result = await _promoCodeManager.ApplyPromoCodeAsync(PromoCode);
            if (result.Success)
            {
                await _vpnInfoUpdater.Update();
                await _eventAggregator.PublishOnUIThreadAsync(
                    new AccountActionMessage(Translation.Get("PromoCode_lbl_Success")));
                ClosePopup();
            }
            else
            {
                Error = result.Failure ? result.Error : string.Empty;
            }

            Sending = false;
        }

        private bool AllowToApplyPromoCode()
        {
            return !Sending && !PromoCode.IsNullOrEmpty();
        }
    }
}