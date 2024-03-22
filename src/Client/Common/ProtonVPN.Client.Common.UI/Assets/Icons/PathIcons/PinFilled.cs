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

public class PinFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.498 12H2.46c-.278 0-.509-.227-.453-.497.3-1.442 1.834-2.296 2.391-2.562a.43.43 0 0 0 .242-.31l.983-5.433a.398.398 0 0 0-.103-.345l-1.114-1.18A.4.4 0 0 1 4.7 1h6.601a.4.4 0 0 1 .294.674l-1.114 1.18a.398.398 0 0 0-.103.344l.983 5.434a.43.43 0 0 0 .241.309c.558.266 2.09 1.117 2.389 2.559.056.27-.175.497-.453.497L8.505 12v3.5c0 .276-.225.5-.504.5a.502.502 0 0 1-.503-.5V12Z";

    protected override string IconGeometry20 { get; }
        = "M9.372 15H3.077c-.348 0-.636-.283-.566-.621.373-1.803 2.292-2.87 2.988-3.203a.538.538 0 0 0 .303-.386L7.03 3.997a.498.498 0 0 0-.129-.43L5.51 2.092a.5.5 0 0 1 .367-.842h8.252a.5.5 0 0 1 .367.842l-1.393 1.475a.498.498 0 0 0-.128.43l1.228 6.793c.03.17.146.31.302.386.697.333 2.612 1.397 2.985 3.199.07.338-.218.621-.566.621L10.631 15v4.375a.627.627 0 0 1-.63.625.627.627 0 0 1-.629-.625V15Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.247 18H3.692c-.417 0-.763-.34-.68-.746.449-2.162 2.752-3.444 3.587-3.843a.646.646 0 0 0 .363-.463l1.474-8.151a.597.597 0 0 0-.154-.517L6.61 2.51a.6.6 0 0 1 .441-1.01h9.902a.6.6 0 0 1 .441 1.01l-1.671 1.77a.597.597 0 0 0-.154.517l1.473 8.15a.646.646 0 0 0 .363.464c.836.4 3.134 1.676 3.582 3.839.084.406-.262.746-.679.746l-7.55.004v5.25c0 .414-.339.75-.756.75a.753.753 0 0 1-.755-.75V18Z"; 

    protected override string IconGeometry32 { get; }
        = "M14.996 24H4.923c-.557 0-1.018-.453-.906-.994.597-2.884 3.668-4.592 4.782-5.125a.861.861 0 0 0 .484-.618l1.965-10.868a.796.796 0 0 0-.206-.688l-2.228-2.36C8.33 2.837 8.696 2 9.402 2h13.202c.706 0 1.07.836.588 1.347l-2.228 2.36a.796.796 0 0 0-.206.688l1.965 10.868c.05.272.233.498.484.618 1.114.533 4.178 2.235 4.776 5.119.112.541-.35.994-.906.994L17.01 24v7c0 .552-.45 1-1.007 1a1.004 1.004 0 0 1-1.007-1v-7Z";
}