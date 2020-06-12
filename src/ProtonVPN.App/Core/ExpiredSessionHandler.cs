using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Login.ViewModels;

namespace ProtonVPN.Core
{
    internal class ExpiredSessionHandler : IVpnStateAware
    {
        private VpnStatus _vpnStatus;
        private readonly IScheduler _scheduler;
        private readonly IVpnServiceManager _vpnServiceManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly UserAuth _userAuth;

        public ExpiredSessionHandler(
            IScheduler scheduler,
            IVpnServiceManager vpnServiceManager,
            LoginViewModel loginViewModel,
            UserAuth userAuth)
        {
            _userAuth = userAuth;
            _loginViewModel = loginViewModel;
            _vpnServiceManager = vpnServiceManager;
            _scheduler = scheduler;
        }

        public async void Execute()
        {
            await _scheduler.Schedule(async () =>
            {
                if (_vpnStatus != VpnStatus.Disconnected)
                {
                    await _vpnServiceManager.Disconnect(VpnError.Unknown);
                }

                _loginViewModel.OnSessionExpired();
                _userAuth.Logout();
            });
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            return Task.CompletedTask;
        }
    }
}
