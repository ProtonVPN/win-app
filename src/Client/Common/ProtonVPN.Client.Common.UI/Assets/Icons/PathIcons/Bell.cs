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

public class Bell : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m13.181 10.68 1.315 1.316.006.006.029.028c.304.305.456.457.466.587a.355.355 0 0 1-.123.298c-.1.085-.315.085-.745.085H1.857c-.424 0-.636 0-.734-.084A.35.35 0 0 1 1 12.623c.01-.13.16-.28.46-.579l.039-.04.008-.007 1.316-1.316a.616.616 0 0 0 .18-.436V5.998a4.998 4.998 0 0 1 9.997 0v4.247c0 .163.065.32.18.436Zm-.707.707.612.613H2.92l.612-.613c.303-.303.473-.714.473-1.142V5.998a3.998 3.998 0 0 1 7.997 0v4.247c0 .428.17.84.473 1.143ZM8.003 15a1 1 0 0 1-1-1h2a1 1 0 0 1-1 1Z";

    protected override string IconGeometry20 { get; }
        = "m16.477 13.35 1.643 1.645.008.008.035.035c.38.38.57.57.583.733a.444.444 0 0 1-.154.373c-.124.106-.393.106-.93.106H2.32c-.53 0-.795 0-.918-.105a.438.438 0 0 1-.152-.367c.013-.16.2-.348.575-.723l.049-.049.01-.01L3.53 13.35a.77.77 0 0 0 .225-.545V7.498a6.248 6.248 0 1 1 12.496 0v5.308c0 .204.081.4.226.545Zm-.885.884.766.766H3.648l.766-.766a2.02 2.02 0 0 0 .591-1.428V7.498a4.998 4.998 0 0 1 9.996 0v5.308c0 .536.213 1.05.591 1.428Zm-5.588 4.516c-.69 0-1.25-.56-1.25-1.25h2.5c0 .69-.56 1.25-1.25 1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "m19.772 16.02 1.972 1.974.01.01.042.042c.456.456.684.684.7.88a.532.532 0 0 1-.186.446c-.149.128-.471.128-1.117.128H2.785c-.636 0-.954 0-1.1-.126a.525.525 0 0 1-.183-.44c.015-.193.24-.418.69-.868l.058-.059.013-.012 1.973-1.974a.925.925 0 0 0 .27-.654v-6.37a7.497 7.497 0 1 1 14.995 0v6.37c0 .245.098.48.27.654Zm-1.061 1.061.918.919H4.38l.918-.919c.454-.454.71-1.071.71-1.714v-6.37a5.997 5.997 0 1 1 11.994 0v6.37c0 .643.255 1.26.71 1.714ZM12.004 22.5a1.5 1.5 0 0 1-1.5-1.5h3a1.5 1.5 0 0 1-1.5 1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "m26.362 21.361 2.63 2.63.013.013.056.057c.608.608.912.912.933 1.173a.71.71 0 0 1-.247.596c-.199.17-.629.17-1.49.17H3.715c-.848 0-1.272 0-1.469-.168a.7.7 0 0 1-.243-.587c.02-.257.32-.557.92-1.157L3 24.01l.017-.017 2.63-2.632a1.23 1.23 0 0 0 .362-.871v-8.494C6.009 6.476 10.484 2 16.005 2c5.521 0 9.997 4.476 9.997 9.996v8.494c0 .326.13.64.36.871Zm-1.414 1.414L26.172 24H5.838l1.224-1.225a3.233 3.233 0 0 0 .947-2.285v-8.494a7.996 7.996 0 0 1 15.992 0v8.494c0 .857.341 1.679.947 2.285ZM16.006 30a2 2 0 0 1-2-2h4a2 2 0 0 1-2 2Z";
}