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

using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using System;

namespace ProtonVPN.Onboarding
{
    public class Onboarding : ILoggedInAware
    {
        private const int StepCount = 4;

        private readonly IAppSettings _appSettings;

        public Onboarding(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public event EventHandler<int> StepChanged;

        public void GoToNextStep()
        {
            if (_appSettings.OnboardingStep < StepCount)
            {
                InvokeStep(++_appSettings.OnboardingStep);
            }
        }

        public void GoToPreviousStep()
        {
            if (_appSettings.OnboardingStep > 0)
            {
                InvokeStep(--_appSettings.OnboardingStep);
            }
        }

        public void Start()
        {
            InvokeStep(1);
        }

        public void Finish()
        {
            _appSettings.OnboardingStep = -1;
            StepChanged?.Invoke(this, 0);
        }

        public bool IsLastStep()
        {
            return _appSettings.OnboardingStep == StepCount;
        }

        public bool IsFirstStep()
        {
            return _appSettings.OnboardingStep == 1;
        }

        public void OnUserLoggedIn()
        {
            int step = _appSettings.OnboardingStep;
            StepChanged?.Invoke(this, step >= 0 ? step : 0);
        }

        private void InvokeStep(int step)
        {
            _appSettings.OnboardingStep = step;
            StepChanged?.Invoke(this,  step);
        }
    }
}
