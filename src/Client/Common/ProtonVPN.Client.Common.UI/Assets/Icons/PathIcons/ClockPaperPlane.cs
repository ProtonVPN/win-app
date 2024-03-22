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

using ProtonVPN.Client.Common.UI.Assets.Icons.Base;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class ClockPaperPlane : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.5 1a6 6 0 1 0 0 12 .5.5 0 0 1 0 1 7 7 0 1 1 6.662-4.846.5.5 0 0 1-.951-.308A6 6 0 0 0 7.5 1Zm2.487 8.382 5.763 2.881a.451.451 0 0 1 0 .808l-5.763 2.88a.452.452 0 0 1-.654-.403v-2.141l2.449-.56c.214-.024.214-.336 0-.36l-2.449-.561v-2.14c0-.336.354-.554.654-.404ZM8 3.5a.5.5 0 0 0-1 0V7a.5.5 0 0 0 .146.354l1.5 1.5a.5.5 0 0 0 .708-.708L8 6.793V3.5Z";

    protected override string IconGeometry20 { get; }
        = "M9.375 1.25a7.5 7.5 0 1 0 0 15 .625.625 0 1 1 0 1.25 8.75 8.75 0 1 1 8.328-6.058.625.625 0 0 1-1.19-.384A7.5 7.5 0 0 0 9.375 1.25Zm3.108 10.477 7.205 3.602a.564.564 0 0 1 0 1.01l-7.204 3.6a.564.564 0 0 1-.817-.504V16.76l3.06-.701c.268-.03.268-.42 0-.449l-3.06-.702v-2.675c0-.42.441-.692.816-.505ZM10 4.375a.625.625 0 1 0-1.25 0V8.75c0 .166.066.325.183.442l1.875 1.875a.625.625 0 1 0 .884-.884L10 8.491V4.375Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.25 1.5a9 9 0 0 0 0 18 .75.75 0 0 1 0 1.5C5.451 21 .75 16.299.75 10.5S5.451 0 11.25 0s10.5 4.701 10.5 10.5c0 1.126-.177 2.212-.507 3.23a.75.75 0 1 1-1.427-.46 8.994 8.994 0 0 0 .434-2.77 9 9 0 0 0-9-9Zm3.73 12.573 8.646 4.322a.677.677 0 0 1 0 1.211l-8.646 4.322a.677.677 0 0 1-.98-.606V20.11l3.672-.84c.322-.036.322-.503 0-.539L14 17.888v-3.21c0-.503.53-.83.98-.605ZM12 5.25a.75.75 0 0 0-1.5 0v5.25c0 .199.079.39.22.53l2.25 2.25a.75.75 0 1 0 1.06-1.06L12 10.19V5.25Z"; 

    protected override string IconGeometry32 { get; }
        = "M15 2C8.373 2 3 7.373 3 14s5.373 12 12 12a1 1 0 1 1 0 2C7.268 28 1 21.732 1 14S7.268 0 15 0s14 6.268 14 14c0 1.501-.237 2.95-.675 4.307a1 1 0 0 1-1.904-.614A11.99 11.99 0 0 0 27 14c0-6.627-5.373-12-12-12Zm4.974 16.764L31.5 24.526a.903.903 0 0 1 0 1.615l-11.527 5.763a.903.903 0 0 1-1.307-.808v-4.282l4.896-1.121c.429-.048.429-.67 0-.718l-4.896-1.124v-4.28a.903.903 0 0 1 1.307-.808ZM16 7a1 1 0 1 0-2 0v7a1 1 0 0 0 .293.707l3 3a1 1 0 0 0 1.414-1.414L16 13.586V7Z";
}