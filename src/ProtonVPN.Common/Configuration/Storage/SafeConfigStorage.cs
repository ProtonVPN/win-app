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

using System;
using Newtonsoft.Json;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Common.Configuration.Storage
{
    public class SafeConfigStorage : IConfigStorage
    {
        private readonly IConfigStorage _origin;

        public SafeConfigStorage(IConfigStorage origin)
        {
            _origin = origin;
        }

        public IConfiguration Value()
        {
            try
            {
                return _origin.Value();
            }
            catch (Exception e) when (e.IsFileAccessException() || e is JsonException)
            {
                return null;
            }
        }

        public void Save(IConfiguration value)
        {
            try
            {
                _origin.Save(value);
            }
            catch (Exception e) when (e.IsFileAccessException() || e is JsonException)
            {
            }
        }
    }
}
