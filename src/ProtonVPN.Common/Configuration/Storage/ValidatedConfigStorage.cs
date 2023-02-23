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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtonVPN.Common.Configuration.Storage
{
    public class ValidatedConfigStorage : IConfigStorage
    {
        private readonly IConfigStorage _origin;

        public ValidatedConfigStorage(IConfigStorage origin)
        {
            _origin = origin;
        }

        public IConfiguration Value()
        {
            IConfiguration value = _origin.Value();
            return Valid(value) ? value : null;
        }

        public void Save(IConfiguration value)
        {
            _origin.Save(value);
        }

        private bool Valid(IConfiguration value)
        {
            return Valid((object) value) &&
                   Valid(value.OpenVpn) &&
                   Valid(value.Urls);
        }

        private bool Valid(object value)
        {
            if (value == null)
            {
                return false;
            }

            ValidationContext context = new(value, null, null);
            List<ValidationResult> results = new();

            return Validator.TryValidateObject(value, context, results, true);
        }
    }
}
