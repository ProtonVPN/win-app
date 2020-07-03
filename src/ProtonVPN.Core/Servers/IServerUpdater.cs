using System;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Servers
{
    public interface IServerUpdater
    {
        Task Update();

        event EventHandler ServersUpdated;
    }
}
