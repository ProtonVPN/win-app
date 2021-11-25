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
using ProtonVPN.Core.Api.Contracts.ReportAnIssue;

namespace ProtonVPN.Core.ReportAnIssue
{
    internal class DefaultCategoryProvider
    {
        public static List<IssueCategory> GetCategories()
        {
            return new List<IssueCategory>
            {
                new()
                {
                    Label = "Slow speed",
                    SubmitLabel = "Slow speed",
                    Suggestions =
                        new List<IssueSuggestion>
                        {
                            new()
                            {
                                Text = "Secure Core slows down your connection. Use it only when necessary.",
                                Link = "https://protonvpn.com/support/secure-core-vpn"
                            },
                            new()
                            {
                                Text = "Switch to another protocol from the settings.",
                                Link = "https://protonvpn.com/support/how-to-change-vpn-protocols"
                            },
                            new()
                            {
                                Text = "Select a server closer to your location."
                            }
                        },
                    InputFields = new List<IssueInput>
                    {
                        new()
                        {
                            Label = "Network type",
                            SubmitLabel = "Network type",
                            Type = InputType.SingleLineInput,
                            Placeholder = "Home WiFi, School Wi-Fi, Work, Public Wi-Fi",
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What are you trying to do?",
                            SubmitLabel = "What are you trying to do?",
                            Type = InputType.MultiLineInput,
                            Placeholder = "Example: browse a website, upload pictures, download a PDF",
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What is the speed you are getting?",
                            SubmitLabel = "What is the speed you are getting?",
                            Type = InputType.SingleLineInput,
                            Placeholder = "Example: 20 KB in download",
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What is your connection speed without VPN?",
                            SubmitLabel = "What is your connection speed without VPN?",
                            Type = InputType.SingleLineInput,
                            Placeholder = "Example: 10 MB in download",
                            IsMandatory = true
                        }
                    }
                },
                new()
                {
                    Label = "Can't connect to VPN",
                    SubmitLabel = "Can't connect to VPN",
                    Suggestions =
                        new List<IssueSuggestion>
                        {
                            new() { Text = "Try connecting to another server." },
                            new()
                            {
                                Text = "Switch to another protocol from the settings.",
                                Link = "https://protonvpn.com/support/how-to-change-vpn-protocols"
                            },
                            new()
                            {
                                Text = "Try disabling any antivirus/firewall to avoid interference with ProtonVPN."
                            }
                        },
                    InputFields = new List<IssueInput>
                    {
                        new()
                        {
                            Label = "Network type",
                            SubmitLabel = "Network type",
                            Type = InputType.SingleLineInput,
                            Placeholder = "Home WiFi, School Wi-Fi, Work, Public Wi-Fi",
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What are the exact steps you performed? Include any error you experienced.",
                            SubmitLabel = "What are the exact steps you performed? Include any error you experienced.",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What have you tried already from the suggestions?",
                            SubmitLabel = "What have you tried already from the suggestions?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        }
                    }
                },
                new()
                {
                    Label = "Connection not stable",
                    SubmitLabel = "Connection not stable",
                    Suggestions =
                        new List<IssueSuggestion>
                        {
                            new() { Text = "Try connecting to another server." },
                            new()
                            {
                                Text = "Switch to another protocol from the settings.",
                                Link = "https://protonvpn.com/support/how-to-change-vpn-protocols"
                            },
                            new()
                            {
                                Text = "Try disabling any antivirus/firewall to avoid interference with ProtonVPN."
                            }
                        },
                    InputFields = new List<IssueInput>
                    {
                        new()
                        {
                            Label = "Network type",
                            SubmitLabel = "Network type",
                            Type = InputType.SingleLineInput,
                            Placeholder = "Home WiFi, School Wi-Fi, Work, Public Wi-Fi",
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "Are you receiving any error message?",
                            SubmitLabel = "Are you receiving any error message?",
                            Type = InputType.MultiLineInput,
                            Placeholder = "Example: an application fails to load content",
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What have you tried already from the suggestions?",
                            SubmitLabel = "What have you tried already from the suggestions?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        }
                    }
                },
                new()
                {
                    Label = "Application issue",
                    SubmitLabel = "Application issue",
                    Suggestions =
                        new List<IssueSuggestion>
                        {
                            new() { Text = "Try logging out/logging in" },
                            new() { Text = "Restart the app" }
                        },
                    InputFields = new List<IssueInput>
                    {
                        new()
                        {
                            Label = "What went wrong?",
                            SubmitLabel = "What went wrong?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What are the exact steps you performed? Include any error you experienced.",
                            SubmitLabel = "What are the exact steps you performed? Include any error you experienced.",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What have you tried already from the suggestions?",
                            SubmitLabel = "What have you tried already from the suggestions?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        }
                    }
                },
                new()
                {
                    Label = "Other",
                    SubmitLabel = "Other",
                    Suggestions = new List<IssueSuggestion>(),
                    InputFields = new List<IssueInput>
                    {
                        new()
                        {
                            Label = "Describe your issue in detail and the steps you took.",
                            SubmitLabel = "Describe your issue in detail and the steps you took.",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        }
                    }
                }
            };
        }
    }
}