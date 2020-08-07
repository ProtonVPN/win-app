using System;

namespace ProtonVPN.Core.Servers
{
    public interface ILastServerLoadTimeProvider
    {
        void Update();

        DateTime LastChecked();
    }
}
