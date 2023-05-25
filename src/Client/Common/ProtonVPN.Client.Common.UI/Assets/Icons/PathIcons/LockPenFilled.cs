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

public class LockPenFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M9 5v-.5a2.5 2.5 0 0 0-5 0V5h5Zm1 .02V4.5a3.5 3.5 0 1 0-7 0v.52c-.392.023-.67.077-.908.198a2 2 0 0 0-.874.874C1 6.52 1 7.08 1 8.2v3.6c0 1.12 0 1.68.218 2.108a2 2 0 0 0 .874.874C2.52 15 3.08 15 4.2 15h4.6c1.12 0 1.68 0 2.108-.218a2 2 0 0 0 .874-.874C12 13.48 12 12.92 12 11.8v-.59l-1.2 1.194a2.121 2.121 0 0 1-1.048.57l-1.178.288-.018.004a1.28 1.28 0 0 1-1.517-1.485l.004-.021.273-1.188a2.12 2.12 0 0 1 .583-1.089l3.7-3.684a2 2 0 0 0-.691-.581c-.237-.121-.516-.175-.908-.199Zm3.327.44c.22-.22.576-.22.796 0l.712.71a.56.56 0 0 1 0 .793l-4.746 4.732A1.127 1.127 0 0 1 9.52 12l-1.181.289a.281.281 0 0 1-.334-.325l.274-1.192c.041-.224.15-.43.311-.591l4.737-4.722Z";
}