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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.MVVM;

namespace ProtonVPN.Onboarding
{
    public class OnboardingViewModel : ViewModel, IOnboardingStepAware
    {
        private readonly Onboarding _onboarding;

        private int _number;
        private bool _isLastStep;
        private bool _isFirstStep;

        public OnboardingViewModel(Onboarding onboarding)
        {
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

        public void OnStepChanged(int step)
        {
            Number = step;
            IsLastStep = _onboarding.IsLastStep();
            IsFirstStep = _onboarding.IsFirstStep();
        }

        private void NextTipAction()
        {
            if (_onboarding.IsLastStep())
                _onboarding.Finish();
            else
                _onboarding.GoToNextStep();
        }

        private void PrevTipAction()
        {
            _onboarding.GoToPreviousStep();
        }
    }
}
