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

using ProtonVPN.Client.Common.UI.Assets.Icons.Base;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class LockCheckFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.2 5H5a3 3 0 0 1 6 0H5.2ZM4 5.02V5a4 4 0 1 1 8 0v.02c.392.023.67.077.908.198a2 2 0 0 1 .874.874C14 6.52 14 7.08 14 8.2v3.6c0 1.12 0 1.68-.218 2.108a2 2 0 0 1-.874.874C12.48 15 11.92 15 10.8 15H5.2c-1.12 0-1.68 0-2.108-.218a2 2 0 0 1-.874-.874C2 13.48 2 12.92 2 11.8V8.2c0-1.12 0-1.68.218-2.108a2 2 0 0 1 .874-.874c.238-.121.516-.175.908-.199Zm7.854 2.626a.5.5 0 0 1 0 .708l-3.859 3.858a.7.7 0 0 1-.99 0l-1.859-1.858a.5.5 0 0 1 .708-.708L7.5 11.293l3.646-3.647a.5.5 0 0 1 .708 0Z";

    protected override string IconGeometry20 { get; }
        = "M6.5 6.25h-.25a3.75 3.75 0 1 1 7.5 0H6.5ZM5 6.274V6.25a5 5 0 0 1 10 0v.024c.49.03.838.097 1.135.248a2.5 2.5 0 0 1 1.092 1.093c.273.535.273 1.235.273 2.635v4.5c0 1.4 0 2.1-.273 2.635a2.5 2.5 0 0 1-1.092 1.092c-.535.273-1.235.273-2.635.273h-7c-1.4 0-2.1 0-2.635-.273a2.5 2.5 0 0 1-1.093-1.092C2.5 16.85 2.5 16.15 2.5 14.75v-4.5c0-1.4 0-2.1.272-2.635a2.5 2.5 0 0 1 1.093-1.093c.297-.15.645-.218 1.135-.248Zm9.817 3.284a.625.625 0 0 1 0 .884l-4.823 4.823a.875.875 0 0 1-1.238 0l-2.323-2.323a.625.625 0 1 1 .884-.884l2.058 2.058 4.558-4.558a.625.625 0 0 1 .884 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M7.8 7.5h-.3a4.5 4.5 0 0 1 9 0H7.8ZM6 7.529V7.5a6 6 0 1 1 12 0v.029c.588.036 1.006.116 1.362.298a3 3 0 0 1 1.311 1.311C21 9.78 21 10.62 21 12.3v5.4c0 1.68 0 2.52-.327 3.162a3 3 0 0 1-1.311 1.311c-.642.327-1.482.327-3.162.327H7.8c-1.68 0-2.52 0-3.162-.327a3 3 0 0 1-1.311-1.311C3 20.22 3 19.38 3 17.7v-5.4c0-1.68 0-2.52.327-3.162a3 3 0 0 1 1.311-1.311c.356-.182.774-.262 1.362-.298Zm11.78 3.94a.75.75 0 0 1 0 1.061l-5.788 5.788a1.05 1.05 0 0 1-1.485 0L7.72 15.53a.75.75 0 1 1 1.06-1.06l2.47 2.47 5.47-5.47a.75.75 0 0 1 1.06 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M10.4 10H10a6 6 0 0 1 12 0H10.4Zm-2.4.038V10a8 8 0 1 1 16 0v.038c.784.048 1.34.156 1.816.398a4 4 0 0 1 1.748 1.748C28 13.04 28 14.16 28 16.4v7.2c0 2.24 0 3.36-.436 4.216a4 4 0 0 1-1.748 1.748C24.96 30 23.84 30 21.6 30H10.4c-2.24 0-3.36 0-4.216-.436a4 4 0 0 1-1.748-1.748C4 26.96 4 25.84 4 23.6v-7.2c0-2.24 0-3.36.436-4.216a4 4 0 0 1 1.748-1.748c.475-.242 1.032-.35 1.816-.398Zm15.707 5.255a1 1 0 0 1 0 1.414l-7.717 7.717a1.4 1.4 0 0 1-1.98 0l-3.717-3.717a1 1 0 1 1 1.414-1.414L15 22.586l7.293-7.293a1 1 0 0 1 1.414 0Z";
}