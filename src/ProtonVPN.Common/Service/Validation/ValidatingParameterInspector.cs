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
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace ProtonVPN.Common.Service.Validation
{
    public class ValidatingParameterInspector : IParameterInspector
    {
        private readonly IEnumerable<IObjectValidator> _validators;

        public ValidatingParameterInspector(IEnumerable<IObjectValidator> validators)
        {
            _validators = validators;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            List<ValidationResult> validationResults = new();

            foreach (object input in inputs)
            {
                foreach (IObjectValidator validator in _validators)
                {
                    IEnumerable<ValidationResult> results = validator.Validate(input);
                    validationResults.AddRange(results.Where(result => result != null));
                }
            }

            if (validationResults.Count > 0)
            {
                throw new FaultException(operationName + " operation failed: " +
                                         validationResults.FirstOrDefault()?.ErrorMessage);
            }

            return null;
        }
    }
}