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
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.FeatureFlags;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Profiles.Servers;
using Profile = ProtonVPN.Core.Profiles.Profile;

namespace ProtonVPN.Profiles.Form
{
    public class B2BProfileFormViewModel : AbstractForm, IVpnPlanAware
    {
        private bool _unsavedChanges;
        
        private List<string> _gateways;
        public List<string> Gateways
        {
            get => _gateways;
            set => Set(ref _gateways, value);
        }
        
        private string _selectedGateway;
        public string SelectedGateway
        {
            get => _selectedGateway;
            set
            {
                if (value != _selectedGateway)
                {
                    if (EditMode && _selectedGateway != null)
                    {
                        _unsavedChanges = true;
                    }

                    _selectedGateway = value;
                    NotifyOfPropertyChange();
                }

                LoadServers();
                SelectedServer = GetSelectedServer();
            }
        }

        public B2BProfileFormViewModel(
            IConfiguration appConfig,
            ColorProvider colorProvider,
            IUserStorage userStorage,
            ProfileManager profileManager,
            IDialogs dialogs,
            IModals modals,
            ServerManager serverManager,
            IProfileFactory profileFactory,
            IFeatureFlagsProvider featureFlagsProvider)
            : base(appConfig, colorProvider, userStorage, profileManager, dialogs, modals, serverManager, profileFactory, featureFlagsProvider)
        {
        }

        public Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            LoadGateways();
            return Task.CompletedTask;
        }

        public override bool HasUnsavedChanges()
        {
            return _unsavedChanges || base.HasUnsavedChanges();
        }

        public override void Load()
        {
            base.Load();
            if (!EditMode)
            {
                LoadGateways();
                LoadServers();
            }
        }

        public override void LoadProfile(Profile profile)
        {
            LoadGateways();
            SelectedGateway = Gateways.FirstOrDefault(g => g.Equals(profile.GatewayName));
            base.LoadProfile(profile);
        }

        public override void Clear()
        {
            base.Clear();
            SelectedGateway = null;
            Gateways = null;
            _unsavedChanges = false;
        }

        protected override Profile CreateProfile()
        {
            Profile profile = base.CreateProfile();
            profile.GatewayName = SelectedGateway;
            profile.ServerId = SelectedServer?.Id;
            return profile;
        }

        protected override async Task<Error> GetFormError()
        {
            Error error = await base.GetFormError();
            if (error != Error.None)
            {
                if (error == Error.EmptyServer && SelectedGateway == null)
                {
                    return Error.EmptyGateway;
                }

                return error;
            }

            if (SelectedGateway == null)
            {
                return Error.EmptyGateway;
            }

            return Error.None;
        }

        protected virtual List<IServerViewModel> GetServersByGateway(string gatewayName)
        {
            Specification<LogicalServerResponse> spec = new ServerByFeatures(GetFeatures()) &&
                                                        new ServerByGateway(gatewayName);
            IReadOnlyCollection<Server> gatewayServers = ServerManager.GetServers(spec);

            List<IServerViewModel> servers = GetPredefinedServerViewModels()
                .Union(GetServerViewModels(gatewayServers))
                .ToList();

            return servers;
        }

        protected override Features GetFeatures()
        {
            return Features.B2B;
        }

        private IEnumerable<string> GetGateways()
        {
            return ServerManager.GetServers(new ServerByFeatures(GetFeatures()))
                .Select(s => s.GatewayName)
                .Distinct();
        }

        private void LoadGateways()
        {
            Gateways = GetGateways().OrderBy(g => g).ToList();
        }

        private void LoadServers()
        {
            Servers = GetServersByGateway(SelectedGateway);
        }

        private IServerViewModel GetSelectedServer()
        {
            if (EditMode)
            {
                return _unsavedChanges
                    ? Servers?.FirstOrDefault()
                    : Servers?.FirstOrDefault(s => s.Name == SelectedServer?.Name);
            }

            return Servers?.FirstOrDefault();
        }
    }
}