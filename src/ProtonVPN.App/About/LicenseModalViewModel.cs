using System;
using System.IO;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Modals;

namespace ProtonVPN.About
{
    public class LicenseModalViewModel : BaseModalViewModel
    {
        private readonly ILogger _logger;
        private const string LicenseFile = "COPYING.md";

        public LicenseModalViewModel(ILogger logger)
        {
            _logger = logger;
        }

        public string License { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            LoadLicense();
        }

        private void LoadLicense()
        {
            try
            {
                License = File.ReadAllText(LicenseFile);
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                _logger.Error<AppFileAccessFailedLog>($"Error when reading license file '{LicenseFile}'.", e);
            }
        }
    }
}
