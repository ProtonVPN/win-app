using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals.Upsell;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Service.Vpn.Connectors
{
    public class ProfileConnector
    {
        private readonly UserStorage _userStorage;
        private readonly AppServerManager _appServerManager;
        private readonly AppSettings _appSettings;
        private readonly ApiServerConnector _apiServerConnector;
        private readonly ModalManager _modalManager;

        public ProfileConnector(
            UserStorage userStorage,
            AppServerManager appServerManager,
            AppSettings appSettings,
            ApiServerConnector apiServerConnector,
            ModalManager modalManager)
        {
            _modalManager = modalManager;
            _userStorage = userStorage;
            _appServerManager = appServerManager;
            _appSettings = appSettings;
            _apiServerConnector = apiServerConnector;
        }

        public async Task Connect(Profile profile)
        {
            var server = GetServerByType(profile);
            if (_appSettings.SecureCore && server == null)
            {
                _modalManager.ShowDialog<ScUpsellModalViewModel>();
                return;
            }

            if (!_appSettings.SecureCore && server == null)
            {
                _modalManager.ShowDialog<PlusUpsellModalViewModel>();
                return;
            }

            if (server.Status == 0)
            {
                ShowMaintenanceServerModal();
                return;
            }

            await _apiServerConnector.Connect(server);
        }

        private ApiServer GetPredefinedProfileServer(Profile profile, sbyte maxTier)
        {
            switch (profile.Name)
            {
                case "Fastest":
                    return _appServerManager.GetFastestServer();
                case "Random":
                    return _appSettings.SecureCore
                        ? _appServerManager.PickRandomSecureCoreServer(maxTier)
                        : _appServerManager.PickRandomStandardServer(maxTier);
                default:
                    return null;
            }
        }

        private void ShowMaintenanceServerModal()
        {
            _modalManager.ShowWarningDialog(Translator.Get("CantConnectToMaintenanceServer"));
        }

        private ApiServer GetServerByType(Profile profile)
        {
            var tier = _userStorage.User().MaxTier;
            var coordinates = _userStorage.Location().GetCoordinates();

            if (profile.IsPredefined)
            {
                return GetPredefinedProfileServer(profile, tier);
            }

            if (!string.IsNullOrEmpty(profile.ServerId))
            {
                return _appServerManager.GetServerById(profile.ServerId);
            }

            if (profile.Features.IsSecureCore())
            {
                switch (profile.ProfileType)
                {
                    case ProfileType.Fastest:
                        return string.IsNullOrEmpty(profile.CountryCode) 
                            ? _appServerManager.PickBestAvailableSecureCoreServer(coordinates, tier)
                            : _appServerManager.PickBestAvailableSecureCoreServer(profile.CountryCode, coordinates, tier);
                    case ProfileType.Random:
                        return string.IsNullOrEmpty(profile.CountryCode) 
                            ? _appServerManager.PickRandomSecureCoreServer(tier)
                            : _appServerManager.PickRandomSecureCoreServer(tier, profile.CountryCode);
                }
            }

            if (profile.Features.SupportsP2P())
            {
                switch (profile.ProfileType)
                {
                    case ProfileType.Fastest:
                        return _appServerManager.PickBestP2PServer(coordinates, tier);
                    case ProfileType.Random:
                        return _appServerManager.PickRandomP2PServer(tier);
                }
            }

            if (profile.Features.SupportsTor())
            {
                switch (profile.ProfileType)
                {
                    case ProfileType.Fastest:
                        return _appServerManager.PickBestTorServer(coordinates, tier);
                    case ProfileType.Random:
                        return _appServerManager.PickRandomTorServer(tier);
                }
            }

            if (profile.Features.IsStandard())
            {
                switch (profile.ProfileType)
                {
                    case ProfileType.Fastest:
                        return _appServerManager.PickBestQuickConnectCountryServer(
                            profile.CountryCode,
                            _userStorage.Location().GetCoordinates(),
                            _userStorage.User().MaxTier);
                    case ProfileType.Random:
                        return _appServerManager.PickRandomStandardServer(_userStorage.User().MaxTier, profile.CountryCode);
                }
            }

            return null;
        }
    }
}
