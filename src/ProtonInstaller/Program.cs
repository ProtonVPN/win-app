/*
 * Copyright (c) 2024 Proton AG
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

using System.Reflection;

namespace ProtonInstaller;

public class Program
{
    private static readonly HttpClient _downloadHttpClient = new(); 

    static async Task Main(string[] args)
    {
        HttpClient versionHttpClient = GetHttpClient();
        IEnumerable<IProtonProduct> products = GetProducts(versionHttpClient, args);
        CommandLineOption shortcutOption = new("CreateDesktopShortcut", args);

        foreach (IProtonProduct product in products)
        {
            await product.InstallAsync(shortcutOption.Exists());
        }
    }

    private static IEnumerable<IProtonProduct> GetProducts(HttpClient verionHttpClient, string[] args)
    {
        CommandLineOption protonDriveOption = new("Drive", args);
        if (protonDriveOption.Exists())
        {
            yield return new ProtonDriveProduct(_downloadHttpClient, verionHttpClient);
        }

        CommandLineOption protonPassOption = new("Pass", args);
        if (protonPassOption.Exists())
        {
            yield return new ProtonPassProduct(_downloadHttpClient, verionHttpClient);
        }

        CommandLineOption protonMailOption = new("Mail", args);
        if (protonMailOption.Exists())
        {
            yield return new ProtonMailProduct(_downloadHttpClient, verionHttpClient);
        }
    }

    private static HttpClient GetHttpClient()
    {
        string? version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

        HttpClient httpClient = new(new CertificateHandler());
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"ProtonVPNInstaller/{version} ({Environment.OSVersion})");
        return httpClient;
    }
}