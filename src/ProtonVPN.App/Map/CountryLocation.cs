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

using System.Collections.Generic;
using System.Windows;

namespace ProtonVPN.Map
{
    public class CountryLocation
    {
        private readonly Dictionary<string, Point> _coordinates = new Dictionary<string, Point>
        {
            { "AD", new Point(337.0, 203.0) },
            { "AE", new Point(426.0, 240.0) },
            { "AF", new Point(447.0, 222.0) },
            { "AL", new Point(369.0, 206.0) },
            { "AM", new Point(412.0, 209.0) },
            { "AO", new Point(367.0, 302.0) },
            { "AR", new Point(225.0, 338.0) },
            { "AT", new Point(358.0, 192.8) },
            { "AU", new Point(566.0, 325.0) },
            { "AZ", new Point(416.0, 209.0) },
            { "BA", new Point(366.0, 200.0) },
            { "BD", new Point(488.0, 240.0) },
            { "BE", new Point(343.0, 186.0) },
            { "BG", new Point(377.0, 203.0) },
            { "BH", new Point(422.0, 236.0) },
            { "BI", new Point(388.0, 286.0) },
            { "BO", new Point(219.0, 311.0) },
            { "BR", new Point(240.0, 300.0) },
            { "BT", new Point(488.0, 233.0) },
            { "BZ", new Point(181.0, 252.0) },
            { "BW", new Point(375.0, 320.0) },
            { "BY", new Point(384.0, 180.0) },
            { "CA", new Point(151.0, 169.0) },
            { "CD", new Point(372.0, 282.0) },
            { "CF", new Point(367.0, 268.0) },
            { "CG", new Point(359.0, 281.0) },
            { "CH", new Point(349.0, 195.0) },
            { "CL", new Point(210.0, 343.0) },
            { "CM", new Point(355.0, 272.0) },
            { "CO", new Point(208.0, 272.0) },
            { "CR", new Point(190.0, 265.0) },
            { "CU", new Point(194.0, 242.5) },
            { "CZ", new Point(360.0, 190.0) },
            { "CY", new Point(392.0, 219.0) },
            { "DE", new Point(351.0, 190.1) },
            { "DJ", new Point(408.0, 261.0) },
            { "DK", new Point(350.0, 176.0) },
            { "DO", new Point(212.0, 249.5) },
            { "DZ", new Point(342.0, 229.0) },
            { "EC", new Point(198.0, 284.0) },
            { "EE", new Point(379.0, 166.5) },
            { "EG", new Point(385.0, 234.0) },
            { "ER", new Point(401.0, 254.0) },
            { "ES", new Point(326.0, 210.0) },
            { "ET", new Point(400.0, 266.0) },
            { "FI", new Point(380.0, 154.0) },
            { "FR", new Point(337.0, 198.0) },
            { "GA", new Point(352.0, 281.0) },
            { "GB", new Point(333.0, 184.0) },
            { "GE", new Point(410.0, 202.0) },
            { "GF", new Point(242.0, 275.0) },
            { "GH", new Point(332.0, 268.0) },
            { "GL", new Point(258.0, 140.0) },
            { "GR", new Point(373.0, 211.0) },
            { "GT", new Point(177.0, 257.0) },
            { "GY", new Point(232.0, 271.0) },
            { "HK", new Point(531.1, 242.1) },
            { "HN", new Point(185.0, 256.0) },
            { "HR", new Point(362.0, 197.0) },
            { "HT", new Point(209.0, 250.0) },
            { "HU", new Point(368.0, 195.1) },
            { "ID", new Point(533.0, 282.0) },
            { "IE", new Point(321.0, 180.0) },
            { "IL", new Point(394.5, 226.0) },
            { "IR", new Point(428.0, 223.0) },
            { "IQ", new Point(409.0, 222.0) },
            { "IN", new Point(470.0, 250.0) },
            { "IS", new Point(304.0, 152.0) },
            { "IT", new Point(358.0, 205.0) },
            { "JM", new Point(201.0, 250.5) },
            { "JO", new Point(397.0, 227.0) },
            { "JP", new Point(575.0, 216.0) },
            { "KE", new Point(400.0, 281.0) },
            { "KG", new Point(456.0, 207.0) },
            { "KH", new Point(516.0, 258.0) },
            { "KP", new Point(553.0, 210.0) },
            { "KR", new Point(555.0, 215.0) },
            { "KZ", new Point(442.0, 190.0) },
            { "KW", new Point(417.0, 229.0) },
            { "LA", new Point(511.0, 248.0) },
            { "LB", new Point(395.0, 222.0) },
            { "LK", new Point(474.0, 269.0) },
            { "LT", new Point(377.0, 177.0) },
            { "LU", new Point(344.0, 188.0) },
            { "LV", new Point(379.0, 172.0) },
            { "LY", new Point(364.0, 232.0) },
            { "MA", new Point(322.0, 224.0) },
            { "MD", new Point(385.0, 193.0) },
            { "ME", new Point(368.0, 203.0) },
            { "MG", new Point(415.0, 315.0) },
            { "MK", new Point(372.0, 204.0) },
            { "ML", new Point(332.0, 252.0) },
            { "MM", new Point(499.0, 247.0) },
            { "MN", new Point(502.0, 197.0) },
            { "MR", new Point(315.0, 247.0) },
            { "MT", new Point(359.3, 218.1) },
            { "MV", new Point(462.0, 275.0) },
            { "MW", new Point(394.0, 302.0) },
            { "MX", new Point(160.0, 238.0) },
            { "MY", new Point(510.0, 273.0) },
            { "MZ", new Point(401.0, 306.0) },
            { "NA", new Point(366.0, 320.0) },
            { "NE", new Point(352.0, 254.0) },
            { "NG", new Point(348.0, 265.0) },
            { "NI", new Point(188.0, 261.0) },
            { "NL", new Point(343.0, 182.1) },
            { "NO", new Point(348.0, 164.0) },
            { "NP", new Point(478.0, 232.0) },
            { "NZ", new Point(633.0, 358.0) },
            { "OM", new Point(434.0, 243.0) },
            { "PA", new Point(197.0, 266.2) },
            { "PE", new Point(204.0, 299.0) },
            { "PG", new Point(584.0, 292.0) },
            { "PH", new Point(548.0, 262.0) },
            { "PK", new Point(451.0, 232.0) },
            { "PL", new Point(369.8, 182.3) },
            { "PR", new Point(220.0, 250.6) },
            { "PS", new Point(395.0, 225.0) },
            { "PT", new Point(320.0, 209.0) },
            { "PY", new Point(237.0, 325.0) },
            { "QA", new Point(423.0, 237.0) },
            { "RO", new Point(379.0, 197.0) },
            { "RS", new Point(370.0, 199.0) },
            { "RU", new Point(397.0, 172.0) },
            { "RW", new Point(388.0, 284.0) },
            { "SA", new Point(416.0, 238.0) },
            { "SD", new Point(382.0, 253.0) },
            { "SE", new Point(363.0, 158.0) },
            { "SG", new Point(513.6, 279.0) },
            { "SI", new Point(360.0, 195.0) },
            { "SK", new Point(369.0, 191.0) },
            { "SN", new Point(308.0, 256.0) },
            { "SO", new Point(419.0, 264.0) },
            { "SR", new Point(237.0, 274.0) },
            { "SS", new Point(386.0, 271.0) },
            { "SV", new Point(181.0, 259.0) },
            { "SY", new Point(400.0, 216.0) },
            { "TD", new Point(367.0, 253.0) },
            { "TH", new Point(509.0, 255.5) },
            { "TJ", new Point(454.0, 212.0) },
            { "TL", new Point(552.0, 297.0) },
            { "TM", new Point(435.0, 210.0) },
            { "TN", new Point(351.0, 217.0) },
            { "TR", new Point(394.0, 208.0) },
            { "TW", new Point(543.2, 240.0) },
            { "TZ", new Point(395.0, 292.0) },
            { "UA", new Point(391.0, 190.0) },
            { "UG", new Point(391.0, 280.0) },
            { "UK", new Point(333.0, 184.0) },
            { "US", new Point(163.0, 215.0) },
            { "UY", new Point(238.0, 341.0) },
            { "UZ", new Point(440.0, 206.0) },
            { "VE", new Point(218.0, 266.0) },
            { "VN", new Point(521.0, 261.0) },
            { "YE", new Point(417.0, 255.0) },
            { "ZA", new Point(373.0, 335.0) },
            { "ZM", new Point(380.0, 307.0) },
            { "ZW", new Point(385.0, 314.0) }
        };

        private readonly string _code;

        public CountryLocation(string code)
        {
            _code = code;
        }

        public Point Coordinates()
        {
            return _coordinates.ContainsKey(_code) ? _coordinates[_code] : new Point(0.0, 0.0);
        }
    }
}
