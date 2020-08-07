using System;

namespace ProtonVPN.Core.Servers
{
    public class LastServerLoadTimeProvider : ILastServerLoadTimeProvider
    {
        private DateTime _lastCheck = DateTime.Now;

        public void Update()
        {
            _lastCheck = DateTime.Now;
        }

        public DateTime LastChecked()
        {
            return _lastCheck;
        }
    }
}
