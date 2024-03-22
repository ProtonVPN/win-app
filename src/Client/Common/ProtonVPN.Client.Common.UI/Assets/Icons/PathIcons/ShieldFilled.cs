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

public class ShieldFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 2.029 3.07 3.723c-.16 1.784-.077 3.66.587 5.403.67 1.761 1.958 3.456 4.343 4.802 2.385-1.346 3.673-3.04 4.343-4.802.664-1.743.748-3.62.587-5.403L8 2.03Zm-.16-1.002-5.4 1.856a.495.495 0 0 0-.332.415c-.418 3.917.17 8.692 5.664 11.645a.488.488 0 0 0 .459 0c5.492-2.953 6.08-7.728 5.663-11.645a.495.495 0 0 0-.333-.415L8.162 1.027a.497.497 0 0 0-.323 0ZM4.222 4.203l3.67-1.325a.31.31 0 0 1 .216 0l3.67 1.325A.35.35 0 0 1 12 4.5c.278 2.798-.185 6.209-3.847 8.317a.307.307 0 0 1-.306 0C4.185 10.71 3.721 7.298 4 4.5a.35.35 0 0 1 .222-.297Z";

    protected override string IconGeometry20 { get; }
        = "M10 2.536 3.838 4.654c-.201 2.23-.096 4.575.733 6.754.838 2.201 2.448 4.32 5.43 6.001 2.98-1.682 4.59-3.8 5.428-6.001.83-2.18.934-4.524.733-6.754L10 2.536Zm-.202-1.252L3.05 3.604a.619.619 0 0 0-.416.518c-.522 4.897.213 10.866 7.08 14.556a.61.61 0 0 0 .573 0c6.866-3.69 7.601-9.66 7.079-14.556a.619.619 0 0 0-.416-.519l-6.748-2.32a.621.621 0 0 0-.404 0Zm-4.52 3.97 4.588-1.657a.388.388 0 0 1 .269 0l4.588 1.657c.151.056.26.2.277.37.348 3.498-.231 7.762-4.809 10.398a.383.383 0 0 1-.382 0C5.231 13.386 4.652 9.122 5 5.625a.437.437 0 0 1 .277-.371Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 3.043 4.606 5.585c-.241 2.675-.115 5.49.88 8.104 1.006 2.642 2.938 5.184 6.516 7.202 3.577-2.018 5.508-4.56 6.514-7.201.995-2.616 1.12-5.43.88-8.105L12 3.043Zm-.242-1.503L3.66 4.324a.743.743 0 0 0-.499.623c-.627 5.876.256 13.038 8.495 17.467a.733.733 0 0 0 .688 0c8.24-4.429 9.122-11.591 8.495-17.467a.743.743 0 0 0-.499-.623L12.243 1.54a.746.746 0 0 0-.485 0ZM6.333 6.305l5.506-1.988a.465.465 0 0 1 .323 0l5.506 1.988A.524.524 0 0 1 18 6.75c.418 4.197-.278 9.313-5.77 12.476a.46.46 0 0 1-.459 0C6.278 16.063 5.582 10.946 6 6.75a.525.525 0 0 1 .333-.445Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 4.058 6.14 7.447c-.321 3.567-.154 7.319 1.174 10.806 1.341 3.521 3.916 6.91 8.687 9.602 4.77-2.691 7.344-6.08 8.685-9.602 1.327-3.487 1.495-7.24 1.174-10.806L16 4.057Zm-.322-2.004L4.88 5.765a.99.99 0 0 0-.665.83c-.836 7.835.34 17.385 11.327 23.29a.977.977 0 0 0 .917 0C27.445 23.98 28.621 14.43 27.785 6.596a.99.99 0 0 0-.665-.83L16.324 2.054a.994.994 0 0 0-.647 0ZM8.442 8.406l7.342-2.65a.62.62 0 0 1 .43 0l7.342 2.65A.7.7 0 0 1 24 9c.557 5.597-.37 12.418-7.694 16.636a.613.613 0 0 1-.611 0C8.37 21.417 7.443 14.595 8 9a.7.7 0 0 1 .443-.593Z";
}