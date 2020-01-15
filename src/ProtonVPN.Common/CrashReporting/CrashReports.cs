using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Service;
using Sentry;

namespace ProtonVPN.Common.CrashReporting
{
    public static class CrashReports
    {
        public static void Init(Config config, ILogger logger = null)
        {
            var options = new SentryOptions
            {
                Release = $"vpn.windows-{config.AppVersion}",
                AttachStacktrace = true,
                Dsn = !string.IsNullOrEmpty(GlobalConfig.SentryDsn) ? new Dsn(GlobalConfig.SentryDsn) : null,
            };

            if (logger != null)
            {
                options.Debug = true;
                options.DiagnosticLogger = new SentryDiagnosticLogger(logger);
            }

            SentrySdk.Init(options);
        }
    }
}
