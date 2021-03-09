using System.Threading.Tasks;

namespace ProtonVPN.Core.Service.Settings
{
    public interface ISettingsServiceClientManager
    {
        Task UpdateServiceSettings();
        Task DisableKillSwitch();
        Task EnableHardKillSwitch();
    }
}