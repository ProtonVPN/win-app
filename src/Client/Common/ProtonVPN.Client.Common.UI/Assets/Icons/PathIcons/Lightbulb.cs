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

public class Lightbulb : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11.025 8.618a4 4 0 1 0-6.05 0c.53.611 1.127 1.416 1.389 2.382h3.272c.262-.966.859-1.77 1.389-2.382ZM5.5 12c0-1.033-.604-1.947-1.28-2.727a5 5 0 1 1 7.56 0c-.676.78-1.28 1.694-1.28 2.727v.5c0 .822-.303 1.464-.796 1.894C9.222 14.815 8.598 15 8 15c-.598 0-1.222-.185-1.704-.606-.493-.43-.796-1.072-.796-1.894V12Zm1 .5V12h3v.5c0 .559-.197.917-.454 1.141C8.778 13.875 8.402 14 8 14s-.778-.125-1.046-.359c-.257-.224-.454-.582-.454-1.141Zm-.5-7A1.5 1.5 0 0 1 7.5 4a.5.5 0 0 0 0-1A2.5 2.5 0 0 0 5 5.5a.5.5 0 0 0 1 0Z";

    protected override string IconGeometry20 { get; }
        = "M13.78 10.772a5 5 0 1 0-7.562 0c.664.765 1.41 1.771 1.737 2.978h4.09c.328-1.207 1.073-2.213 1.736-2.978ZM6.876 15c0-1.291-.755-2.434-1.6-3.41a6.25 6.25 0 1 1 9.451 0c-.846.976-1.601 2.119-1.601 3.41v.625c0 1.027-.379 1.83-.995 2.368-.603.526-1.383.757-2.13.757-.747 0-1.527-.23-2.13-.757-.616-.538-.995-1.34-.995-2.368V15Zm1.25.625V15h3.75v.625c0 .699-.246 1.146-.567 1.426-.335.293-.805.449-1.308.449s-.973-.156-1.308-.449c-.32-.28-.567-.727-.567-1.426ZM7.5 6.875C7.5 5.839 8.34 5 9.375 5a.625.625 0 1 0 0-1.25A3.125 3.125 0 0 0 6.25 6.875a.625.625 0 1 0 1.25 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.537 12.927a6 6 0 1 0-9.074 0c.795.917 1.69 2.125 2.083 3.573h4.908c.393-1.448 1.288-2.656 2.083-3.573ZM8.25 18c0-1.55-.906-2.92-1.92-4.091a7.5 7.5 0 1 1 11.341 0C16.656 15.079 15.75 16.45 15.75 18v.75c0 1.233-.454 2.196-1.194 2.842-.723.631-1.66.908-2.556.908-.896 0-1.832-.277-2.556-.908-.74-.646-1.194-1.61-1.194-2.842V18Zm1.5.75V18h4.5v.75c0 .838-.296 1.375-.68 1.712-.402.35-.966.538-1.57.538-.604 0-1.168-.188-1.57-.538-.384-.337-.68-.874-.68-1.712ZM9 8.25A2.25 2.25 0 0 1 11.25 6a.75.75 0 0 0 0-1.5A3.75 3.75 0 0 0 7.5 8.25a.75.75 0 0 0 1.5 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M22.05 17.235A7.961 7.961 0 0 0 24 12a8 8 0 1 0-16 0c0 2.004.734 3.831 1.95 5.235 1.06 1.224 2.254 2.834 2.778 4.765h6.544c.524-1.931 1.717-3.541 2.777-4.765ZM11 24c0-2.066-1.208-3.894-2.56-5.455A9.961 9.961 0 0 1 6 12C6 6.477 10.477 2 16 2s10 4.477 10 10a9.96 9.96 0 0 1-2.44 6.545C22.209 20.106 21 21.935 21 24v1c0 1.644-.606 2.928-1.592 3.789C18.443 29.63 17.195 30 16 30c-1.195 0-2.443-.37-3.408-1.211C11.606 27.928 11 26.644 11 25v-1Zm2 1v-1h6v1c0 1.118-.394 1.834-.908 2.282-.535.468-1.287.718-2.092.718-.805 0-1.557-.25-2.092-.718C13.394 26.834 13 26.118 13 25Zm-1-14a3 3 0 0 1 3-3 1 1 0 1 0 0-2 5 5 0 0 0-5 5 1 1 0 1 0 2 0Z";
}