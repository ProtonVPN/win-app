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
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Profiles.Servers;
using Profile = ProtonVPN.Core.Profiles.Profile;

namespace ProtonVPN.Profiles.Form
{
    public abstract class BaseCountryServerProfileFormViewModel : AbstractForm, IVpnPlanAware
    {
        private CountryViewModel _selectedCountry;
        private List<CountryViewModel> _countries;
        private readonly IModals _modals;
        private bool _unsavedChanges;

        protected BaseCountryServerProfileFormViewModel(
            IConfiguration appConfig,
            ColorProvider colorProvider,
            IUserStorage userStorage,
            ProfileManager profileManager,
            IDialogs dialogs,
            IModals modals,
            ServerManager serverManager,
            IProfileFactory profileFactory)
            : base(appConfig, colorProvider, userStorage, profileManager, dialogs, modals, serverManager, profileFactory)
        {
            _modals = modals;
        }

        public List<CountryViewModel> Countries
        {
            get => _countries;
            set => Set(ref _countries, value);
        }

        public CountryViewModel SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (value?.CountryCode != _selectedCountry?.CountryCode)
                {
                    if (ShowUpgradeModal(value))
                    {
                        _modals.Show<UpsellModalViewModel>();
                    }

                    if (EditMode && _selectedCountry != null)
                    {
                        _unsavedChanges = true;
                    }

                    _selectedCountry = value;
                    NotifyOfPropertyChange();
                }

                LoadServers();
                SelectedServer = GetSelectedServer();
            }
        }

        public Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            LoadCountries();
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
                LoadCountries();
                LoadServers();
            }
        }

        public override void LoadProfile(Profile profile)
        {
            LoadCountries();
            SelectedCountry = Countries.FirstOrDefault(c => c.CountryCode.Equals(profile.CountryCode));
            base.LoadProfile(profile);
        }

        public override void Clear()
        {
            base.Clear();
            SelectedCountry = null;
            Countries = null;
            _unsavedChanges = false;
        }

        protected override Profile CreateProfile()
        {
            Profile profile = base.CreateProfile();
            profile.CountryCode = SelectedCountry?.CountryCode;
            profile.ServerId = SelectedServer?.Id;
            return profile;
        }

        protected override async Task<Error> GetFormError()
        {
            Error error = await base.GetFormError();
            if (error != Error.None)
            {
                if (error == Error.EmptyServer && SelectedCountry == null)
                {
                    return Error.EmptyCountry;
                }

                return error;
            }

            if (SelectedCountry == null)
            {
                return Error.EmptyCountry;
            }

            return Error.None;
        }

        protected virtual List<IServerViewModel> GetServersByCountry(string countryCode)
        {
            Specification<LogicalServerResponse> spec = new ServerByFeatures(GetFeatures()) &&
                                                        new ExitCountryServer(countryCode);
            IReadOnlyCollection<Server> countryServers = ServerManager.GetServers(spec);

            List<IServerViewModel> servers = GetPredefinedServerViewModels()
                .Union(GetServerViewModels(countryServers))
                .ToList();

            return servers;
        }

        private IEnumerable<string> GetCountries()
        {
            return ServerManager.GetServers(new ServerByFeatures(GetFeatures()))
                .Select(s => s.ExitCountry)
                .Distinct();
        }

        private bool IsUpgradeRequiredForCountry(string countryCode)
        {
            Specification<LogicalServerResponse> spec = new ServerByFeatures(GetFeatures()) &&
                                                        new ExitCountryServer(countryCode) &&
                                                        new MaxTierServer(UserStorage.GetUser().MaxTier);

            return !ServerManager.GetServers(spec).Any();
        }

        private void LoadCountries()
        {
            Countries = GetCountries()
                .OrderBy(ProtonVPN.Servers.Countries.GetName)
                .Select(c => new CountryViewModel(c, IsUpgradeRequiredForCountry(c)))
                .ToList();
        }

        private void LoadServers()
        {
            Servers = GetServersByCountry(SelectedCountry?.CountryCode);
        }

        private bool ShowUpgradeModal(CountryViewModel value)
        {
            if (value != null && value.UpgradeRequired)
            {
                if (EditMode)
                {
                    if (SelectedCountry != null && !SelectedCountry.UpgradeRequired)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
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
