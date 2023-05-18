/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using ProtonVPN.Client.Localization.Contracts;
using WinUI3Localizer;

namespace ProtonVPN.Client.Localization;

public class LocalizationBuilder : ILocalizationBuilder
{
    public async Task BuildAsync()
    {
        LocalizerBuilder builder = new();

        string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

        foreach (string resourceName in resourceNames)
        {
            string language = GetLanguageFolderName(resourceName);
            if (string.IsNullOrEmpty(language))
            {
                continue;
            }

            builder.AddLanguageDictionary(BuildLanguageDictionary(language, resourceName));
        }

        builder.SetOptions(options =>
        {
            options.DefaultLanguage = "en-US";
            options.UseUidWhenLocalizedStringNotFound = true;
        });

        await builder.Build();
    }

    private string GetLanguageFolderName(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            return string.Empty;
        }

        return resourceName.Split('.').SkipLast(2).Last().Replace("_", "-");
    }

    private LanguageDictionary BuildLanguageDictionary(string language, string resourceName)
    {
        LanguageDictionary languageDictionary = new(language);

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);

        XmlDocument xmlDocument = new();
        xmlDocument.Load(reader);
        XmlNodeList nodes = xmlDocument.GetElementsByTagName("data");

        foreach (XmlNode node in nodes)
        {
            string name = node.Attributes?["name"]?.Value;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            string value = node["value"]?.InnerText ?? string.Empty;

            languageDictionary.AddItem(
                new LanguageDictionary.Item(name, string.Empty, value, name));
        }

        return languageDictionary;
    }
}