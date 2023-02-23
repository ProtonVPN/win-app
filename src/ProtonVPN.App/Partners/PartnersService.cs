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
using System.Linq;
using ProtonVPN.Api.Contracts.Partners;

namespace ProtonVPN.Partners
{
    public class PartnersService : IPartnersService
    {
        private List<PartnerType> _partnerTypes = new();

        public PartnersService(IPartnersUpdater partnersUpdater)
        {
            partnersUpdater.PartnersUpdated += OnPartnersUpdated;
        }

        public List<PartnerType> GetPartnerTypes()
        {
            return _partnerTypes;
        }

        private void OnPartnersUpdated(object sender, List<PartnerTypeResponse> partnerTypes)
        {
            _partnerTypes = Map(partnerTypes);
        }

        private Partner Map(PartnerResponse partnerResponse)
        {
            return new Partner
            {
                Name = partnerResponse.Name,
                Description = partnerResponse.Description,
                IconUrl = partnerResponse.IconUrl,
                WebsiteUrl = partnerResponse.WebsiteUrl,
                LogicalIDs = partnerResponse.LogicalIDs,
            };
        }

        private List<Partner> Map(List<PartnerResponse> response)
        {
            return response.Select(Map).ToList();
        }

        private List<PartnerType> Map(List<PartnerTypeResponse> partnerTypesResponse)
        {
            return partnerTypesResponse.Select(partnerType => new PartnerType
                {
                    Type = partnerType.Type,
                    Description = partnerType.Description,
                    IconUrl = partnerType.IconUrl,
                    Partners = Map(partnerType.Partners),
                })
                .ToList();
        }
    }
}