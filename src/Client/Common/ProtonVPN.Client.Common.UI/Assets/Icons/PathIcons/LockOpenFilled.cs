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

public class LockOpenFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 0a4 4 0 0 0-4 4v1.02c-.392.023-.67.077-.908.198a2 2 0 0 0-.874.874C2 6.52 2 7.08 2 8.2v3.6c0 1.12 0 1.68.218 2.108a2 2 0 0 0 .874.874C3.52 15 4.08 15 5.2 15h5.6c1.12 0 1.68 0 2.108-.218a2 2 0 0 0 .874-.874C14 13.48 14 12.92 14 11.8V8.2c0-1.12 0-1.68-.218-2.108a2 2 0 0 0-.874-.874C12.48 5 11.92 5 10.8 5H5V4a3 3 0 0 1 5.572-1.546.5.5 0 0 0 .856-.516A3.998 3.998 0 0 0 8 0Zm.47 9.883a1 1 0 1 0-.94 0l-.437 1.744a.3.3 0 0 0 .291.373h1.232a.3.3 0 0 0 .29-.373l-.435-1.744Z";

    protected override string IconGeometry20 { get; }
        = "M10 0a5 5 0 0 0-5 5v1.274c-.49.03-.838.097-1.135.248a2.5 2.5 0 0 0-1.093 1.093C2.5 8.15 2.5 8.85 2.5 10.25v4.5c0 1.4 0 2.1.272 2.635a2.5 2.5 0 0 0 1.093 1.092c.535.273 1.235.273 2.635.273h7c1.4 0 2.1 0 2.635-.273a2.5 2.5 0 0 0 1.092-1.092c.273-.535.273-1.235.273-2.635v-4.5c0-1.4 0-2.1-.273-2.635a2.5 2.5 0 0 0-1.092-1.093C15.6 6.25 14.9 6.25 13.5 6.25H6.25V5a3.75 3.75 0 0 1 6.965-1.932.625.625 0 1 0 1.07-.645A4.998 4.998 0 0 0 10 0Zm.588 12.353a1.25 1.25 0 1 0-1.176 0l-.546 2.181A.375.375 0 0 0 9.23 15h1.54c.244 0 .423-.23.364-.466l-.546-2.18Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 0a6 6 0 0 0-6 6v1.529c-.588.036-1.006.116-1.362.298a3 3 0 0 0-1.311 1.311C3 9.78 3 10.62 3 12.3v5.4c0 1.68 0 2.52.327 3.162a3 3 0 0 0 1.311 1.311c.642.327 1.482.327 3.162.327h8.4c1.68 0 2.52 0 3.162-.327a3 3 0 0 0 1.311-1.311C21 20.22 21 19.38 21 17.7v-5.4c0-1.68 0-2.52-.327-3.162a3 3 0 0 0-1.311-1.311C18.72 7.5 17.88 7.5 16.2 7.5H7.5V6a4.5 4.5 0 0 1 8.358-2.319.75.75 0 0 0 1.284-.774A5.997 5.997 0 0 0 12 0Zm.706 14.824a1.5 1.5 0 1 0-1.412 0l-.654 2.617a.45.45 0 0 0 .436.559h1.848a.45.45 0 0 0 .436-.56l-.654-2.616Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 0a8 8 0 0 0-8 8v2.038c-.784.048-1.341.156-1.816.398a4 4 0 0 0-1.748 1.748C4 13.04 4 14.16 4 16.4v7.2c0 2.24 0 3.36.436 4.216a4 4 0 0 0 1.748 1.748C7.04 30 8.16 30 10.4 30h11.2c2.24 0 3.36 0 4.216-.436a4 4 0 0 0 1.748-1.748C28 26.96 28 25.84 28 23.6v-7.2c0-2.24 0-3.36-.436-4.216a4 4 0 0 0-1.748-1.748C24.96 10 23.84 10 21.6 10H10V8a6 6 0 0 1 11.143-3.091 1 1 0 1 0 1.713-1.033A7.996 7.996 0 0 0 16 0Zm.941 19.765a2 2 0 1 0-1.882 0l-.873 3.49a.6.6 0 0 0 .582.745h2.463a.6.6 0 0 0 .583-.745l-.873-3.49Z";
}