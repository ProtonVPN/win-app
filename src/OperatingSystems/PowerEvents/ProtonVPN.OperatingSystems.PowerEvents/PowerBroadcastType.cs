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

namespace ProtonVPN.OperatingSystems.PowerEvents;

public enum PowerBroadcastType
{
    PBT_APMQUERYSUSPEND = 0, // DEPRECATED: Only used by Windows XP
    PBT_APMQUERYSTANDBY = 1,
    PBT_APMQUERYSUSPENDFAILED = 2, // DEPRECATED: Only used by Windows XP
    PBT_APMQUERYSTANDBYFAILED = 3,
    PBT_APMSUSPEND = 4, // Sent when the computer goes to sleep
    PBT_APMSTANDBY = 5,
    PBT_APMRESUMECRITICAL = 6, // DEPRECATED: Only used by Windows XP
    PBT_APMRESUMESUSPEND = 7, // Sent when the computer wakes up from sleep due to a user input
    PBT_APMRESUMESTANDBY = 8,
    PBT_APMBATTERYLOW = 9,
    PBT_APMPOWERSTATUSCHANGE = 10,
    PBT_APMOEMEVENT = 11,
    PBT_APMRESUMEAUTOMATIC = 18, // Sent when the computer wakes up from sleep
    PBT_POWERSETTINGCHANGE = 32787,
    ERROR_ERROR = 10101
}

// When the computer wakes up due to a user input, both PBT_APMRESUMEAUTOMATIC and then PBT_APMRESUMESUSPEND events are sent