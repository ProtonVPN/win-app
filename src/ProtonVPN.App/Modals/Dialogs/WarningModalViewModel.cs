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

using ProtonVPN.Core.Modals;

namespace ProtonVPN.Modals.Dialogs
{
    public class WarningModalViewModel : BaseModalViewModel
    {
        private string _message;
        private string _primaryButtonText;
        private string _secondaryButtonText;

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            set => Set(ref _primaryButtonText, value);
        }

        public string SecondaryButtonText
        {
            get => _secondaryButtonText;
            set => Set(ref _secondaryButtonText, value);
        }

        public void ApplySettings(IDialogSettings settings)
        {
            Message = settings.Message;
            PrimaryButtonText = settings.PrimaryButtonText;
            SecondaryButtonText = settings.SecondaryButtonText;
        }
    }
}
