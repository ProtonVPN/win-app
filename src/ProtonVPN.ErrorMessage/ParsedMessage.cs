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

namespace ProtonVPN.ErrorMessage
{
    internal class ParsedMessage
    {
        private readonly string[] _messages;
        private const string UnknownError = "Unknown error";

        public ParsedMessage(string[] messages)
        {
            _messages = messages;
        }

        public override string ToString()
        {
            return _messages.Length == 0 ? UnknownError : GetMessage();
        }

        private string GetMessage()
        {
            return "Missing file: " + _messages[0];
        }
    }
}
