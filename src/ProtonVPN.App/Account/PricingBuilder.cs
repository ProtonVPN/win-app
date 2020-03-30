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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Servers;

namespace ProtonVPN.Account
{
    public class PricingBuilder
    {
        private readonly Common.Configuration.Config _appConfig;
        private readonly IApiClient _api;
        private readonly ServerManager _serverManager;
        private Dictionary<string, Pricing> _monthlyPlans = new Dictionary<string, Pricing>();
        private Dictionary<string, Pricing> _yearlyPlans = new Dictionary<string, Pricing>();
        private float _mostExpensivePlanPrice;
        

        public PricingBuilder(Common.Configuration.Config appConfig, IApiClient api, ServerManager serverManager)
        {
            _appConfig = appConfig;
            _api = api;
            _serverManager = serverManager;
        }

        public async Task Load()
        {
            await LoadMonthlyPriceInfo();
            await LoadYearlyPriceInfo();
            SetMostExpensivePlanPrice();
        }

        public List<PricingModel> BuildPricing()
        {
            var totalFreeCountries = GetFreeCountriesCount();
            var pricing = GetInitialPricing(totalFreeCountries);
            var totalCountries = _serverManager.GetCountries().Count;

            foreach (var monthlyPlan in _monthlyPlans)
            {
                if (_yearlyPlans.ContainsKey(monthlyPlan.Key))
                {
                    var plan = monthlyPlan.Value;
                    var yearlyPrice = _yearlyPlans[monthlyPlan.Key].Amount;
                    var countries = plan.Amount == 0 ? totalFreeCountries : totalCountries;

                    pricing.Add(new PricingModel(
                        plan.Name,
                        plan.Amount,
                        yearlyPrice,
                        countries,
                        plan.MaxVpn,
                        GetPlanCompletness(plan)));
                }
            }

            return pricing;
        }

        private static List<PricingModel> GetInitialPricing(int totalFreeCountries)
        {
            return new List<PricingModel>
            {
                new PricingModel("free", 0, 0, totalFreeCountries, 1, .1f)
            };
        }

        private async Task LoadMonthlyPriceInfo()
        {
            try
            {
                var response = await _api.GetPricing(_appConfig.DefaultCurrency, 1);
                if (response.Success)
                    _monthlyPlans = GetPricingDictionary(response.Value);
            }
            catch (HttpRequestException)
            {
                throw new PricingBuilderException();
            }
        }

        private async Task LoadYearlyPriceInfo()
        {
            try
            {
                var response = await _api.GetPricing(_appConfig.DefaultCurrency, 12);
                if (response.Success)
                    _yearlyPlans = GetPricingDictionary(response.Value);
            }
            catch (HttpRequestException)
            {
                throw new PricingBuilderException();
            }
        }

        private static Dictionary<string, Pricing> GetPricingDictionary(PricingPlans pricing)
        {
            return pricing.Plans.Where(plan => PlanIncludesVpn(plan) && IsPlanPrimary(plan)).ToDictionary(plan => plan.Id);
        }

        private static bool PlanIncludesVpn(Pricing plan)
        {
            return (plan.Services & 4) > 0;
        }

        private static bool IsPlanPrimary(Pricing plan)
        {
            return plan.Type == 1;
        }

        private int GetFreeCountriesCount()
        {
            //FIXME: this must be replaced by a proper value from api
            return 3;
        }

        private float GetPlanCompletness(Pricing pricing)
        {
            return pricing.Amount / _mostExpensivePlanPrice;
        }

        private void SetMostExpensivePlanPrice()
        {
            foreach (var plan in _monthlyPlans)
            {
                if (plan.Value.Amount > _mostExpensivePlanPrice)
                    _mostExpensivePlanPrice = plan.Value.Amount;
            }
        }
    }
}
