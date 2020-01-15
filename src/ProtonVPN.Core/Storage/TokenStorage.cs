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

using System.Runtime.CompilerServices;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.OS.Crypto;

namespace ProtonVPN.Core.Storage
{
    public class TokenStorage : ITokenStorage
    {
        private readonly ISettingsStorage _storage;

        public TokenStorage(ISettingsStorage storage)
        {
            _storage = storage;
        }

        public string AccessToken
        {
            get => Get();
            set => Set(value);
        }

        public string RefreshToken
        {
            get => Get();
            set => Set(value);
        }

        public string Uid
        {
            get => Get();
            set => Set(value);
        }

        private void Set(string value, [CallerMemberName] string propertyName = null)
        {
            _storage.Set(propertyName, value.Encrypt());
        }

        private string Get([CallerMemberName] string propertyName = null)
        {
            var value = _storage.Get<string>(propertyName);
            return string.IsNullOrEmpty(value) ? string.Empty : value.Decrypt();
        }
    }
}
