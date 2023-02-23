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

using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Servers;
using ProtonVPN.Translations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtonVPN.Profiles.Servers
{
    public class ServerTypeViewModel : ViewModel
    {
        private static readonly List<Features> AllTypes = new List<Features>
        {
            Features.None,          // "Standard"
            Features.SecureCore,    
            Features.P2P,           
            Features.Tor           
        };

        public string Name => GetName(Features);
        public Features Features { get; }

        protected ServerTypeViewModel(Features features)
        {
            Features = features;
        }

        public static IEnumerable<ServerTypeViewModel> AllServerTypes()
        {
            return AllTypes.Select(f => new ServerTypeViewModel(f));
        }

        public static string TypeName(Features features)
        {
            foreach (var typeFeatures in AllTypes)
            {
                if ((features & typeFeatures) != 0)
                {
                    return GetName(typeFeatures);
                }
            }

            return GetName(Features.None);
        }

        private static string GetName(Features features)
        {
            var enumName = Enum.GetName(typeof(Features), features);
            return Translation.Get($"ServerType_val_{enumName}");
        }
    }
}
