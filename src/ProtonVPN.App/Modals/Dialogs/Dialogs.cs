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

using System.Threading.Tasks;
using ProtonVPN.Core.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.Modals.Dialogs
{
    public class Dialogs : IDialogs
    {
        private readonly IModals _modals;
        private readonly QuestionModalViewModel _questionViewModel;
        private readonly WarningModalViewModel _warningViewModel;

        public Dialogs(IModals modals, WarningModalViewModel warningViewModel, QuestionModalViewModel questionViewModel)
        {
            _modals = modals;
            _warningViewModel = warningViewModel;
            _questionViewModel = questionViewModel;
        }

        public async Task<bool?> ShowWarningAsync(string message)
        {
            DialogSettings settings = DialogSettings.FromMessage(message)
                .WithPrimaryButtonText(Translation.Get("Dialogs_btn_Close"));

            _warningViewModel.ApplySettings(settings);
            return await _modals.ShowAsync<WarningModalViewModel>();
        }

        public async Task<bool?> ShowWarningAsync(string message, string buttonLabel)
        {
            DialogSettings settings = DialogSettings.FromMessage(message).WithPrimaryButtonText(buttonLabel);
            _warningViewModel.ApplySettings(settings);
            return await _modals.ShowAsync<WarningModalViewModel>();
        }

        public async Task<bool?> ShowQuestionAsync(string message)
        {
            DialogSettings settings = DialogSettings.FromMessage(message);
            _questionViewModel.ApplySettings(settings);
            return await _modals.ShowAsync<QuestionModalViewModel>();
        }

        public async Task<bool?> ShowQuestionAsync(IDialogSettings settings)
        {
            _questionViewModel.ApplySettings(settings);
            return await _modals.ShowAsync<QuestionModalViewModel>();
        }
    }
}
