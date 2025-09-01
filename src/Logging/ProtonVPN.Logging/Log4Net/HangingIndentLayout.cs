/*
 * Copyright (c) 2025 Proton AG
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

using System.Text;
using log4net.Core;
using log4net.Layout;

namespace ProtonVPN.Logging.Log4Net;

public sealed class HangingIndentLayout : LayoutSkeleton
{
    private readonly ILayout _inner;

    public HangingIndentLayout(ILayout inner)
    {
        _inner = inner;

        IgnoresException = false;
    }

    public override void ActivateOptions()
    {
        if (_inner is IOptionHandler optionHandler)
        {
            optionHandler.ActivateOptions();
        }
    }

    public override void Format(TextWriter writer, LoggingEvent loggingEvent)
    {
        using (StringWriter buffer = new())
        {
            _inner.Format(buffer, loggingEvent);

            string text = buffer.ToString();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (text.IndexOfAny(['\r', '\n']) < 0)
            {
                writer.Write(text);
                return;
            }

            writer.Write(NormalizeNewlinesWithSpaces(text));
        }
    }

    private static string NormalizeNewlinesWithSpaces(string text)
    {
        StringBuilder sb = new();
        int length = text.Length;

        for (int i = 0; i < length; i++)
        {
            char c = text[i];

            sb.Append(c);

            if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < length && text[i + 1] == '\n')
                {
                    sb.Append('\n');
                    i++;
                }

                if (i + 1 < length)
                {
                    sb.Append("    ");
                }
            }
        }

        return sb.ToString();
    }
}