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
using System.Linq;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
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
            IList<IssueInputResponse> inputs = _reportAnIssueFormDataProvider.GetInputs(categorySubmitName);
            List<FormElement> formElements = new() { GetUsernameField(), GetEmailField() };
            formElements.AddRange(inputs.Select(MapField).Where(element => element != null));
            return formElements;
        }

        private FormElement GetUsernameField()
        {
            return new UsernameInput
            {
                SubmitLabel = "username",
                Placeholder = Translation.Get("BugReport_lbl_UsernamePlaceholder"),
                Label = Translation.Get("BugReport_lbl_Username"),
                IsMandatory = false
            };
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

        private FormElement MapField(IssueInputResponse inputResponse)
        {
            switch (inputResponse.Type)
            {
                case InputType.SingleLineInput:
                    return CreateSingleLineTextField(inputResponse);
                case InputType.MultiLineInput:
                    return CreateMultiLineTextField(inputResponse);
                default:
                    _logger.Info<AppLog>($"Unknown input type {inputResponse.Type}. This field won't be added to the form.");
                    return null;
            }
        }

        private FormElement CreateSingleLineTextField(IssueInputResponse inputResponse)
        {
            return new SingleLineTextInput
            {
                Label = inputResponse.Label,
                Placeholder = inputResponse.Placeholder,
                SubmitLabel = inputResponse.SubmitLabel,
                IsMandatory = inputResponse.IsMandatory
            };
        }

        private FormElement CreateMultiLineTextField(IssueInputResponse inputResponse)
        {
            return new MultiLineTextInput
            {
                Label = inputResponse.Label,
                Placeholder = inputResponse.Placeholder,
                SubmitLabel = inputResponse.SubmitLabel,
                IsMandatory = inputResponse.IsMandatory
            };
        }
    }
}