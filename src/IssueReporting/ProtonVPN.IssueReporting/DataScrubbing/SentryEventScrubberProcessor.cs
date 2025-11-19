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

using System.Collections.Generic;
using System.Linq;
using Sentry;
using Sentry.Extensibility;

namespace ProtonVPN.IssueReporting.DataScrubbing;

public class SentryEventScrubberProcessor : ISentryEventProcessor
{
    private readonly SentryDataScrubber _dataScrubber;

    public SentryEventScrubberProcessor()
    {
        _dataScrubber = new SentryDataScrubber();
    }

    public SentryEvent Process(SentryEvent sEvent)
    {
        try
        {
            if (sEvent.Message != null)
            {
                sEvent.Message = new SentryMessage
                {
                    Message = _dataScrubber.Scrub(sEvent.Message.Message),
                    Formatted = _dataScrubber.Scrub(sEvent.Message.Formatted)
                };
            }

            if (sEvent.Tags.Count > 0)
            {
                Dictionary<string, string?> updated = sEvent.Tags
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => _dataScrubber.Scrub(kvp.Value));

                foreach (KeyValuePair<string, string?> kvp in updated)
                {
                    sEvent.SetTag(kvp.Key, kvp.Value!);
                }
            }

            if (sEvent.Extra.Count > 0)
            {
                foreach (KeyValuePair<string, object?> kvp in sEvent.Extra.ToList())
                {
                    switch (kvp.Value)
                    {
                        case string s:
                            sEvent.SetExtra(kvp.Key, _dataScrubber.Scrub(s));
                            break;

                        case IReadOnlyDictionary<string, object> roDict:
                        {
                            Dictionary<string, object> scrubbed = ScrubDictionary(roDict);
                            sEvent.SetExtra(kvp.Key, scrubbed);
                            break;
                        }
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        return sEvent;
    }

    private Dictionary<string, object> ScrubDictionary(IReadOnlyDictionary<string, object> roDict)
    {
        Dictionary<string, object> scrubbed = new(roDict.Count);
        foreach (KeyValuePair<string, object> inner in roDict)
        {
            if (inner.Value is string innerValueString)
            {
                scrubbed[inner.Key] = _dataScrubber.Scrub(innerValueString)!;
            }
            else
            {
                scrubbed[inner.Key] = inner.Value;
            }
        }

        return scrubbed;
    }
}