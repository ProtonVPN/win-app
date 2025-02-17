/*
 * Copyright (c) 2024 Proton AG
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
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using ProtonVPN.Client.Common.Models;

namespace ProtonVPN.Client.Common.UI.Helpers;

public class RichTextHelper
{
    private const string BOLD_TAG = "**";

    public static Paragraph ParseRichText(string text, InlineTextButton? button = null)
    {
        Paragraph paragraph = new();
        int start = 0;

        while (start < text.Length)
        {
            int openTag = text.IndexOf(BOLD_TAG, start, StringComparison.Ordinal);
            if (openTag == -1)
            {
                // No more tags, add the rest of the text as a normal run
                paragraph.Inlines.Add(new Run { Text = text.Substring(start) });
                break;
            }

            // Add text before the tag as a normal run
            if (openTag > start)
            {
                paragraph.Inlines.Add(new Run { Text = text.Substring(start, openTag - start) });
            }

            int closeTag = text.IndexOf(BOLD_TAG, openTag + BOLD_TAG.Length, StringComparison.Ordinal);
            if (closeTag == -1)
            {
                // Malformed text, treat the rest as normal text
                paragraph.Inlines.Add(new Run { Text = text.Substring(openTag) });
                break;
            }

            // Add bolded text
            int boldTextStart = openTag + BOLD_TAG.Length;
            int boldTextLength = closeTag - boldTextStart;
            paragraph.Inlines.Add(new Run
            {
                Text = text.Substring(boldTextStart, boldTextLength),
                FontWeight = FontWeights.Bold,
            });

            start = closeTag + BOLD_TAG.Length;
        }

        if (button is not null)
        {
            paragraph.Inlines.Add(new Run { Text = " " });

            Hyperlink hyperlink = new()
            {
                NavigateUri = new Uri(button.Url),
                Inlines = { new Run { Text = button.Text } }
            };

            hyperlink.SetValue(ToolTipService.ToolTipProperty, button.Url);

            paragraph.Inlines.Add(hyperlink);
        }

        return paragraph;
    }
}