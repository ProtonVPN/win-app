using System;
using System.Collections.Generic;
using System.IO;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;

namespace ProtonVPN.Core
{
    public class LanguageProvider : ILanguageProvider
    {
        private const string ResourceFile = "ProtonVPN.Translations.resources.dll";

        private readonly ILogger _logger;
        private readonly string _translationsFolder;
        private readonly string _defaultLocale;

        public LanguageProvider(ILogger logger, string translationsFolder, string defaultLocale)
        {
            _logger = logger;
            _defaultLocale = defaultLocale;
            _translationsFolder = translationsFolder;
        }

        public List<string> GetAll()
        {
            try
            {
                return InternalGetAll();
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                _logger.Error<AppFileAccessFailedLog>("Couldn't get language file.", e);
                return new List<string>{ _defaultLocale };
            }
        }

        private List<string> InternalGetAll()
        {
            var langs = new List<string> { _defaultLocale };
            string[] files = Directory.GetFiles(_translationsFolder, ResourceFile, SearchOption.AllDirectories);

            foreach (string file in files)
            {
                var dirInfo = new DirectoryInfo(file);
                if (dirInfo.Parent != null)
                {
                    langs.Add(dirInfo.Parent.Name);
                }
            }

            return langs;
        }
    }
}
