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
using ProtonVPN.Api.Contracts.ReportAnIssue;

namespace ProtonVPN.Core.ReportAnIssue
{
    internal class DefaultCategoryProvider
    {
        public static List<IssueCategoryResponse> GetCategories()
        {
            return new List<IssueCategoryResponse>
            {
                new()
                {
                    Label = "Browsing speed",
                    SubmitLabel = "Browsing speed",
                    Suggestions =
                        new List<IssueSuggestionResponse>
                        {
                            new()
                            {
                                Text = "Turn off Secure Core. It can sometimes slow down your connection.",
                                Link = "https://protonvpn.com/support/secure-core-vpn"
                            },
                            new()
                            {
                                Text = "Use another VPN protocol.",
                                Link = "https://protonvpn.com/support/how-to-change-vpn-protocols"
                            },
                            new()
                            {
                                Text = "Try a different server. Servers in nearby countries often have faster connection speeds."
                            }
                        },
                    InputFields = new List<IssueInputResponse>
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
                    Label = "Connecting to VPN",
                    SubmitLabel = "Connecting to VPN",
                    Suggestions =
                        new List<IssueSuggestionResponse>
                        {
                            new() { Text = "Try a different server. Servers in nearby countries often have faster connection speeds." },
                            new()
                            {
                                Text = "Switch to another protocol from the settings.",
                                Link = "https://protonvpn.com/support/how-to-change-vpn-protocols"
                            },
                            new()
                            {
                                Text = "Turn off any antivirus or firewall software. They could be interfering with your VPN connection."
                            }
                        },
                    InputFields = new List<IssueInputResponse>
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
                            Label = "What steps have you taken to try to connect to VPN?",
                            SubmitLabel = "What steps have you taken to try to connect to VPN?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "Have you tried any quick fixes? If so, which ones?",
                            SubmitLabel = "Have you tried any quick fixes? If so, which ones?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        }
                    }
                },
                new()
                {
                    Label = "Weak or unstable connection",
                    SubmitLabel = "Weak or unstable connection",
                    Suggestions =
                        new List<IssueSuggestionResponse>
                        {
                            new() { Text = "Try a different server. Servers in nearby countries often have faster connection speeds." },
                            new()
                            {
                                Text = "Use another VPN protocol.",
                                Link = "https://protonvpn.com/support/how-to-change-vpn-protocols"
                            },
                            new()
                            {
                                Text = "Turn off any antivirus or firewall software. They could be interfering with your VPN connection."
                            }
                        },
                    InputFields = new List<IssueInputResponse>
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
                            Label = "Did you get an error message? If so, what did it say?",
                            SubmitLabel = "Did you get an error message? If so, what did it say?",
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
                    Label = "Using the app",
                    SubmitLabel = "Using the app",
                    Suggestions =
                        new List<IssueSuggestionResponse>
                        {
                            new() { Text = "Log out and log back in." },
                            new() { Text = "Restart the app." }
                        },
                    InputFields = new List<IssueInputResponse>
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
                            Label = "Have you tried any quick fixes? If so, which ones?",
                            SubmitLabel = "Have you tried any quick fixes? If so, which ones?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        }
                    }
                },
                new()
                {
                    Label = "Streaming",
                    SubmitLabel = "Streaming",
                    Suggestions =
                        new List<IssueSuggestionResponse>
                        {
                            new()
                            {
                                Text = "Try a different PLUS server. Just so you know, streaming is only available on a PLUS subscription."
                            },
                            new()
                            {
                                Text = "Clear your cache. You can do this in your browser settings.",
                                Link = "https://protonvpn.com/support/clear-browser-cache-cookies"
                            },
                            new()
                            {
                                Text = "Try a different web browser."
                            },
                            new()
                            {
                                Text = "Turn off any ad blockers or proxies. If you use NetShield, set it to \"don’t block\"."
                            },
                        },
                    InputFields = new List<IssueInputResponse>
                    {
                        new()
                        {
                            Label = "Which streaming service are you trying to use?",
                            SubmitLabel = "Which streaming service are you trying to use?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true,
                            Placeholder = "Netflix, Disney+, HBO"
                        },
                        new()
                        {
                            Label = "Are you using a custom DNS, proxy, or NetShield?",
                            SubmitLabel = "Are you using a custom DNS, proxy, or NetShield?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        },
                        new()
                        {
                            Label = "What went wrong? Include any error messages you received.",
                            SubmitLabel = "What went wrong? Include any error messages you received.",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true
                        },
                    }
                },
                new()
                {
                    Label = "Something else",
                    SubmitLabel = "Something else",
                    Suggestions = new List<IssueSuggestionResponse>(),
                    InputFields = new List<IssueInputResponse>
                    {
                        new()
                        {
                            Label = "What went wrong?",
                            SubmitLabel = "What went wrong?",
                            Type = InputType.MultiLineInput,
                            IsMandatory = true,
                            Placeholder = "Please describe the problem in as much detail as you can. If there was an error message, let us know what it said."
                        }
                    }
                }
            };
        }
    }
}