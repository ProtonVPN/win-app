/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Linq;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Api.Contracts.ReportAnIssue;
using ProtonVPN.Core.ReportAnIssue;
using ProtonVPN.Translations;

namespace ProtonVPN.BugReporting.FormElements
{
    public class FormElementBuilder : IFormElementBuilder
    {
        private readonly ILogger _logger;
        private readonly IReportAnIssueFormDataProvider _reportAnIssueFormDataProvider;

        public FormElementBuilder(ILogger logger, IReportAnIssueFormDataProvider reportAnIssueFormDataProvider)
        {
            _logger = logger;
            _reportAnIssueFormDataProvider = reportAnIssueFormDataProvider;
        }

        public List<FormElement> GetFormElements(string categorySubmitName)
        {
            IList<IssueInput> inputs = _reportAnIssueFormDataProvider.GetInputs(categorySubmitName);
            List<FormElement> formElements = new() { GetEmailField() };
            formElements.AddRange(inputs.Select(MapField).Where(element => element != null));
            return formElements;
        }

        private FormElement GetEmailField()
        {
            return new EmailInput
            {
                SubmitLabel = "email",
                Placeholder = Translation.Get("BugReport_lbl_EmailPlaceholder"),
                Label = Translation.Get("BugReport_lbl_Email"),
                IsMandatory = true
            };
        }

        private FormElement MapField(IssueInput input)
        {
            switch (input.Type)
            {
                case InputType.SingleLineInput:
                    return CreateSingleLineTextField(input);
                case InputType.MultiLineInput:
                    return CreateMultiLineTextField(input);
                default:
                    _logger.Info($"[FormElementBuilder] Unknown input type {input.Type}. This field won't be added to the form.");
                    return null;
            }
        }

        private FormElement CreateSingleLineTextField(IssueInput input)
        {
            return new SingleLineTextInput
            {
                Label = input.Label,
                Placeholder = input.Placeholder,
                SubmitLabel = input.SubmitLabel,
                IsMandatory = input.IsMandatory
            };
        }

        private FormElement CreateMultiLineTextField(IssueInput input)
        {
            return new MultiLineTextInput
            {
                Label = input.Label,
                Placeholder = input.Placeholder,
                SubmitLabel = input.SubmitLabel,
                IsMandatory = input.IsMandatory
            };
        }
    }
}