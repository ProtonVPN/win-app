using System;
using System.Collections.Generic;
using ProtonVPN.Common.Logging;

namespace ProtonVPN.BugReporting.NetworkLogs
{
    public class NetworkLogWriter
    {
        private readonly IEnumerable<ILog> _networkLogs;
        private readonly ILogger _logger;

        public NetworkLogWriter(IEnumerable<ILog> networkLogs, ILogger logger)
        {
            _logger = logger;
            _networkLogs = networkLogs;
        }

        public void Write()
        {
            foreach (var log in _networkLogs)
            {
                try
                {
                    log.Write();
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to create log file {log.Path}: " + e);
                }
            }
        }
    }
}
