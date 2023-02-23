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
using ProtonVPN.Translations;

namespace ProtonVPN.Modals.Dialogs
{
    public class DialogSettings : IDialogSettings
    {
        public string Message { get; }
        public string PrimaryButtonText { get; }
        public string SecondaryButtonText { get; }

        private DialogSettings(string message, string primaryButtonText, string secondaryButtonText)
        {
            Message = message;
            PrimaryButtonText = primaryButtonText;
            SecondaryButtonText = secondaryButtonText;
        }

        public static DialogSettings FromMessage(string message)
        {
            return new DialogSettings(message, Translation.Get("Dialogs_btn_Continue"), Translation.Get("Dialogs_btn_Cancel"));
        }

        public DialogSettings WithPrimaryButtonText(string text)
        {
            return new DialogSettings(Message, text, SecondaryButtonText);
        }

        public DialogSettings WithSecondaryButtonText(string text)
        {
            return new DialogSettings(Message, PrimaryButtonText, text);
        }
    }
}
