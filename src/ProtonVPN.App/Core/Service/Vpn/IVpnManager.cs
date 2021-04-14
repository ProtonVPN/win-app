using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public interface IVpnManager
    {
        Task ConnectAsync(Profile profile, Profile fallbackProfile = null);

        Task QuickConnectAsync();

        Task ReconnectAsync(VpnReconnectionSettings settings = null);

        Task DisconnectAsync(VpnError vpnError = VpnError.None);

        Task GetStateAsync();

        void OnVpnStateChanged(VpnStateChangedEventArgs e);

        event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;
    }
}
