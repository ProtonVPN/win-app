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

using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Onboarding
{
    public class OnboardingViewModel : ViewModel, IOnboardingStepAware, ISettingsAware
    {
        private readonly Onboarding _onboarding;
        private readonly IAppSettings _appSettings;

        private int _number;
        private bool _isLastStep;
        private bool _isFirstStep;
        private bool _isFeatureNetShieldEnabled;
        private bool _isFeaturePortForwardingEnabled;

        public OnboardingViewModel(Onboarding onboarding, IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _onboarding = onboarding;
            NextTipCommand = new RelayCommand(NextTipAction);
            PrevTipCommand = new RelayCommand(PrevTipAction);
        }

        public ICommand NextTipCommand { get; set; }
        public ICommand PrevTipCommand { get; set; }

        public int Number
        {
            get => _number;
            set => Set(ref _number, value);
        }

        public bool IsLastStep
        {
            get => _isLastStep;
            set => Set(ref _isLastStep, value);
        }

        public bool IsFirstStep
        {
            get => _isFirstStep;
            set => Set(ref _isFirstStep, value);
        }

        public bool IsFeatureNetShieldEnabled
        {
            get => _isFeatureNetShieldEnabled;
            set => Set(ref _isFeatureNetShieldEnabled, value);
        }

        public bool IsFeaturePortForwardingEnabled
        {
            get => _isFeaturePortForwardingEnabled;
            set => Set(ref _isFeaturePortForwardingEnabled, value);
        }

        public void OnStepChanged(int step)
        {
            Number = step;
            IsLastStep = _onboarding.IsLastStep();
            IsFirstStep = _onboarding.IsFirstStep();
            if (step == 4)
            {
                SetIsNetShieldFeatureEnabled();
                SetIsPortForwardingFeatureEnabled();
            }
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAppSettings.FeatureNetShieldEnabled))
            {
                SetIsNetShieldFeatureEnabled();
            }
            if (e.PropertyName == nameof(IAppSettings.FeaturePortForwardingEnabled))
            {
                SetIsPortForwardingFeatureEnabled();
            }
        }

        private void NextTipAction()
        {
            if (_onboarding.IsLastStep())
            {
                _onboarding.Finish();
            }
            else
            {
                _onboarding.GoToNextStep();
            }
        }

        private void PrevTipAction()
        {
            _onboarding.GoToPreviousStep();
        }

        private void SetIsNetShieldFeatureEnabled()
        {
            IsFeatureNetShieldEnabled = _appSettings.FeatureNetShieldEnabled;
        }

        private void SetIsPortForwardingFeatureEnabled()
        {
            IsFeaturePortForwardingEnabled = _appSettings.FeaturePortForwardingEnabled;
        }
    }
}