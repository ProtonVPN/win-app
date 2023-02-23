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

using ProtonVPN.Common.Configuration.Storage;

namespace ProtonVPN.Common.Configuration.Source
{
    public class CustomConfig : IConfigSource
    {
        private readonly ConfigMode _mode;
        private readonly IConfigSource _default;
        private readonly IConfigStorage _storage;

        public CustomConfig(ConfigMode mode, IConfigSource defaultSource, IConfigStorage storage)
        {
            _mode = mode;
            _default = defaultSource;
            _storage = storage;
        }

        public IConfiguration Value()
        {
            if (_mode == ConfigMode.Default)
            {
                return _default.Value();
            }
            
            IConfiguration value = _storage.Value();
            if (value != null)
            {
                return value;
            }

            value = _default.Value();
            if (_mode == ConfigMode.CustomOrSavedDefault)
            {
                _storage.Save(value);
            }

            return value;
        }
    }
}
