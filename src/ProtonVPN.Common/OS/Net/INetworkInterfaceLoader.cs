using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.Common.OS.Net
{
    public interface INetworkInterfaceLoader
    {
        INetworkInterface GetTapInterface();
        INetworkInterface GetTunInterface();
    }
}