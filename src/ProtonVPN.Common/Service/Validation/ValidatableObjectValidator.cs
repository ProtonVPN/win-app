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
using System.Linq;
using ProtonVPN.Common.Configuration;

namespace ProtonVPN.Common.Service.Validation
{
    public class ValidatableObjectValidator : IObjectValidator
    {
        public const string ServerValidationPublicKeyValue = "ServerValidationPublicKey";

        private readonly IConfiguration _config;

        public ValidatableObjectValidator(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<ValidationResult> Validate(object input)
        {
            if (input is IValidatableObject validatableInput)
            {
                Dictionary<object, object> items = new Dictionary<object, object>
                {
                    { ServerValidationPublicKeyValue, _config.ServerValidationPublicKey }
                };
                return validatableInput.Validate(new ValidationContext(input, null, items));
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}