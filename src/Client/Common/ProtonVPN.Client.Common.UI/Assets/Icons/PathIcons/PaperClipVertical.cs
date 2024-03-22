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

public class PaperClipVertical : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6.5 2A2.5 2.5 0 0 0 4 4.5V11a4 4 0 0 0 8 0V5a.5.5 0 0 1 1 0v6a5 5 0 0 1-10 0V4.5a3.5 3.5 0 1 1 7 0V11a2 2 0 1 1-4 0V5a.5.5 0 0 1 1 0v6a1 1 0 1 0 2 0V4.5A2.5 2.5 0 0 0 6.5 2Z";

    protected override string IconGeometry20 { get; }
        = "M11.238 5.273c.028-1.516-1.146-2.696-2.609-2.668-1.484.029-2.732 1.287-2.76 2.82l-.143 7.776c-.044 2.374 1.8 4.237 4.107 4.193 2.328-.044 4.272-2.011 4.316-4.404l.126-6.842a.676.676 0 1 1 1.35.025l-.126 6.842c-.057 3.104-2.57 5.675-5.64 5.734-3.091.059-5.54-2.45-5.483-5.573l.143-7.775c.041-2.245 1.857-4.108 4.084-4.15 2.248-.043 4.026 1.783 3.985 4.047l-.143 7.775c-.026 1.386-1.145 2.54-2.529 2.566-1.404.027-2.512-1.116-2.486-2.521l.126-6.842a.676.676 0 1 1 1.35.025l-.126 6.842c-.012.656.49 1.153 1.11 1.141.64-.012 1.193-.56 1.205-1.236l.143-7.775Z"; 

    protected override string IconGeometry24 { get; }
        = "M13.486 6.327c.033-1.818-1.376-3.234-3.131-3.2-1.78.033-3.279 1.543-3.313 3.384l-.171 9.33c-.052 2.85 2.161 5.085 4.929 5.032 2.793-.053 5.127-2.414 5.18-5.285l.15-8.21a.811.811 0 0 1 .825-.799.812.812 0 0 1 .795.828l-.151 8.21c-.068 3.725-3.084 6.812-6.768 6.882-3.71.07-6.649-2.94-6.58-6.688l.172-9.33c.05-2.694 2.229-4.93 4.901-4.98 2.697-.051 4.831 2.14 4.781 4.856l-.171 9.33c-.03 1.664-1.374 3.048-3.035 3.08-1.685.032-3.014-1.34-2.983-3.025l.151-8.211a.811.811 0 0 1 .825-.798.812.812 0 0 1 .795.828l-.151 8.21c-.014.788.589 1.384 1.333 1.37.768-.014 1.43-.673 1.445-1.483l.172-9.33Z"; 

    protected override string IconGeometry32 { get; }
        = "M17.98 8.436c.045-2.424-1.833-4.312-4.174-4.267-2.374.045-4.371 2.058-4.416 4.512l-.229 12.44c-.07 3.8 2.882 6.78 6.572 6.71 3.724-.07 6.836-3.218 6.906-7.047l.201-10.948a1.082 1.082 0 0 1 1.1-1.064c.596.011 1.07.506 1.06 1.104l-.201 10.948c-.091 4.966-4.112 9.08-9.025 9.174-4.946.095-8.864-3.92-8.772-8.916l.228-12.44c.066-3.593 2.972-6.573 6.535-6.64 3.597-.07 6.442 2.853 6.375 6.474l-.228 12.44c-.04 2.218-1.832 4.064-4.046 4.106-2.247.043-4.019-1.786-3.978-4.033l.201-10.948a1.082 1.082 0 0 1 1.1-1.064 1.082 1.082 0 0 1 1.06 1.104l-.201 10.948c-.02 1.05.785 1.845 1.777 1.826 1.024-.02 1.908-.898 1.927-1.978l.229-12.44Z";
}