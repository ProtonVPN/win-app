/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Resources;

namespace ProtonVPN.Account
{
    public class PricingModel
    {
        private readonly string _name;

        public PricingModel(string name, int monthlyPrice, int yearlyPrice, int countries, int devices, float planCompletness)
        {
            _name = name;
            MonthlyPrice = monthlyPrice;
            YearlyPrice = yearlyPrice;
            Countries = countries;
            Devices = devices;
            PlanCompletness = planCompletness;
        }

        public int MonthlyPrice { get; }
        public int YearlyPrice { get; }
        public int Countries { get; }
        public int Devices { get; }
        public float PlanCompletness { get; }

        public bool IsFree => MonthlyPrice == 0 && YearlyPrice == 0;

        public bool IsVisionary => _name == "visionary";

        public string ServerAccessText => StringResources.Get($"Account_ServerAccess_val_{_name.FirstCharToUpper()}");

        public string CountriesText => StringResources.Format("Account_lbl_Countries", Countries);

        public string MonthlyPriceText => IsFree 
            ? StringResources.Get("Account_lbl_MonthlyPriceZero") 
            : $"${MonthlyPrice / 100}";

        public string YearlyPriceText => StringResources.Format("Account_lbl_YearlyPrice", "$", YearlyPrice / 100);

        public string TitleColor
        {
            get
            {
                switch (_name)
                {
                    case "vpnbasic": return "#fb805f";
                    case "vpnplus": return "#d3f421";
                    case "visionary": return "#5edefb";
                    default: return "White";
                }
            }
        }

        public string DevicesText => StringResources.Format("Account_lbl_Devices", Devices);

        public string Title => _name.ToUpper().Replace("VPN", "");
    }
}
