using System;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core.Service.Vpn
{
    public interface IVpnManager
    {
        Task Connect(Profile profile);

        Task Reconnect();

        Task Disconnect(VpnError vpnError = VpnError.None);

        Task GetState();

        void OnVpnStateChanged(VpnStateChangedEventArgs e);

        event EventHandler<VpnStateChangedEventArgs> VpnStateChanged;
    }
}
