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

public class ExclamationTriangleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14.9 15H1.1c-.843 0-1.372-.894-.954-1.613l6.9-11.844c.422-.724 1.486-.724 1.908 0l6.9 11.844c.418.719-.11 1.613-.955 1.613ZM8 4.757c.622 0 1.104.535 1.027 1.14l-.482 3.779a.546.546 0 0 1-.545.472.546.546 0 0 1-.545-.472l-.482-3.778A1.022 1.022 0 0 1 8 4.757Zm.77 7.008a.762.762 0 0 1-.77.755.762.762 0 0 1-.77-.755c0-.417.345-.755.77-.755.425 0 .77.338.77.755Z";

    protected override string IconGeometry20 { get; }
        = "M18.624 18.75H1.376c-1.055 0-1.716-1.118-1.193-2.016L8.807 1.929c.528-.905 1.858-.905 2.386 0l8.624 14.805c.523.898-.138 2.016-1.193 2.016ZM10 5.946c.778 0 1.38.669 1.284 1.426l-.602 4.723a.683.683 0 0 1-.682.59.683.683 0 0 1-.682-.59l-.601-4.723C8.62 6.615 9.222 5.946 10 5.946Zm.962 8.76c0 .521-.43.944-.962.944a.953.953 0 0 1-.962-.944c0-.52.43-.943.962-.943.531 0 .962.422.962.943Z"; 

    protected override string IconGeometry24 { get; }
        = "M22.349 22.5H1.65c-1.266 0-2.06-1.341-1.431-2.42l10.35-17.765c.632-1.087 2.23-1.087 2.862 0l10.35 17.765c.627 1.079-.166 2.42-1.432 2.42ZM12 7.135c.934 0 1.656.803 1.54 1.712l-.722 5.667a.819.819 0 0 1-.818.708.82.82 0 0 1-.818-.708l-.722-5.667c-.116-.909.606-1.712 1.54-1.712Zm1.154 10.513c0 .625-.517 1.132-1.154 1.132a1.143 1.143 0 0 1-1.154-1.133c0-.625.517-1.132 1.154-1.132.637 0 1.154.507 1.154 1.133Z"; 

    protected override string IconGeometry32 { get; }
        = "M29.798 30H2.202C.514 30-.545 28.211.292 26.774l13.8-23.687c.843-1.45 2.973-1.45 3.817 0l13.798 23.687c.838 1.437-.22 3.226-1.909 3.226ZM16 9.514c1.245 0 2.208 1.07 2.053 2.282l-.962 7.556a1.093 1.093 0 0 1-1.09.944 1.092 1.092 0 0 1-1.092-.944l-.962-7.556c-.155-1.212.808-2.282 2.053-2.282Zm1.54 14.016c0 .834-.69 1.51-1.539 1.51-.85 0-1.539-.676-1.539-1.51s.69-1.51 1.54-1.51c.849 0 1.538.676 1.538 1.51Z";
}