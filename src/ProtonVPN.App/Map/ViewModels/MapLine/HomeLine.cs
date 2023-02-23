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

using ProtonVPN.Map.ViewModels.Pins;

namespace ProtonVPN.Map.ViewModels.MapLine
{
    public class HomeLine : MapLine
    {
        private readonly AbstractPinViewModel _pin;

        public string EntryNodeCountry { get; set; }

        public HomeLine(AbstractPinViewModel pin)
        {
            _pin = pin;
            EntryNodeCountry = pin.CountryCode;
            SetEntryNodePoint(1);
        }

        public override void ApplyMapScale(double scale)
        {
            SetEntryNodePoint(scale);
        }

        private void SetEntryNodePoint(double scale)
        {
            X2 = _pin.HorizontalOffset + _pin.Width / 2;
            Y2 = _pin.VerticalOffset + _pin.Height - 1 / scale;
            if (_pin is SecureCorePinViewModel sc)
            {
                Y2 -= sc.PinHeight / scale / 2;
            }
        }
    }
}
