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

public class Shield : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3.07 3.723c-.16 1.784-.077 3.66.587 5.403.67 1.761 1.958 3.456 4.343 4.802 2.385-1.346 3.673-3.04 4.343-4.802.664-1.743.748-3.62.587-5.403L8 2.03 3.07 3.723Zm-.63-.84 5.399-1.856a.497.497 0 0 1 .323 0l5.398 1.856a.495.495 0 0 1 .333.415c.417 3.917-.17 8.692-5.663 11.645a.488.488 0 0 1-.459 0C2.278 11.99 1.689 7.215 2.107 3.298a.495.495 0 0 1 .333-.415Z";

    protected override string IconGeometry20 { get; }
        = "M3.838 4.654c-.201 2.23-.096 4.575.733 6.754.838 2.201 2.448 4.32 5.43 6.001 2.98-1.682 4.59-3.8 5.428-6.001.83-2.18.934-4.524.733-6.754L10 2.536 3.838 4.654Zm-.788-1.05 6.748-2.32a.621.621 0 0 1 .404 0l6.748 2.32c.227.077.39.278.416.518.522 4.897-.213 10.866-7.079 14.556a.61.61 0 0 1-.573 0c-6.867-3.69-7.602-9.66-7.08-14.556a.619.619 0 0 1 .416-.519Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.605 5.585c-.241 2.675-.115 5.49.88 8.104 1.006 2.642 2.938 5.184 6.516 7.202 3.577-2.018 5.508-4.56 6.514-7.201.995-2.616 1.12-5.43.88-8.105L12 3.043 4.605 5.585ZM3.66 4.324l8.098-2.784a.746.746 0 0 1 .485 0l8.097 2.784a.743.743 0 0 1 .499.623c.627 5.876-.255 13.038-8.494 17.467a.733.733 0 0 1-.689 0c-8.24-4.429-9.122-11.591-8.495-17.467a.743.743 0 0 1 .499-.623Z"; 

    protected override string IconGeometry32 { get; }
        = "M6.14 7.447c-.321 3.567-.154 7.319 1.174 10.806 1.341 3.521 3.916 6.91 8.687 9.602 4.77-2.691 7.344-6.08 8.685-9.602 1.327-3.487 1.495-7.24 1.174-10.806L16 4.057l-9.86 3.39ZM4.88 5.765l10.798-3.711a.994.994 0 0 1 .646 0L27.12 5.765a.99.99 0 0 1 .665.83c.836 7.835-.34 17.385-11.326 23.29a.977.977 0 0 1-.917 0C4.555 23.98 3.379 14.43 4.215 6.596a.99.99 0 0 1 .665-.83Z";
}