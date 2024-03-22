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

public class PenSlash : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 14a.5.5 0 0 1-.707 0L2.5 4.207a.5.5 0 0 1 .707-.707L13 13.293A.5.5 0 0 1 13 14Z m12.02 2.512 1.47 1.464a.032.032 0 0 1 0 .047L9.305 8.19l.707.707 4.183-4.165a1.032 1.032 0 0 0 0-1.464l-1.47-1.465a1.043 1.043 0 0 0-1.47 0L7.077 5.962l.707.707 4.175-4.157a.037.037 0 0 1 .013-.009.046.046 0 0 1 .017-.003c.006 0 .012.001.017.003a.037.037 0 0 1 .012.009ZM7.076 7.375l-.707-.708-3.864 3.849a2.067 2.067 0 0 0-.575 1.09l-.421 2.283a.519.519 0 0 0 .616.6l2.253-.463a2.082 2.082 0 0 0 1.05-.564l3.876-3.859-.707-.707-3.874 3.857c-.15.149-.339.25-.546.293l-1.555.32.291-1.579a1.08 1.08 0 0 1 .297-.563l3.866-3.85Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 17.5a.625.625 0 0 1-.884 0L3.125 5.259a.625.625 0 1 1 .884-.884L16.25 16.616a.625.625 0 0 1 0 .884Z m15.024 3.14 1.838 1.83a.04.04 0 0 1 0 .06l-5.23 5.208.883.884 5.23-5.207a1.29 1.29 0 0 0 0-1.83l-1.839-1.83a1.304 1.304 0 0 0-1.838 0l-5.22 5.197.883.884L14.95 3.14a.047.047 0 0 1 .016-.011.057.057 0 0 1 .021-.004.06.06 0 0 1 .021.004c.005.002.01.005.016.01ZM8.845 9.218l-.883-.884-4.831 4.81a2.585 2.585 0 0 0-.719 1.364l-.526 2.853a.649.649 0 0 0 .77.75l2.817-.579a2.603 2.603 0 0 0 1.312-.704l4.845-4.824-.884-.884-4.843 4.822a1.353 1.353 0 0 1-.682.366l-1.943.4.363-1.974a1.34 1.34 0 0 1 .372-.703l4.832-4.813Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 21a.75.75 0 0 1-1.06 0L3.75 6.31a.75.75 0 0 1 1.06-1.06L19.5 19.94a.75.75 0 0 1 0 1.06Z m18.029 3.768 2.206 2.196c.02.02.02.05 0 .07l-6.277 6.251 1.06 1.061 6.275-6.248c.61-.607.61-1.59 0-2.197l-2.206-2.196a1.564 1.564 0 0 0-2.206 0l-6.264 6.238 1.06 1.06 6.263-6.235a.056.056 0 0 1 .02-.013.068.068 0 0 1 .024-.005c.01 0 .02.002.026.005a.056.056 0 0 1 .019.013Zm-7.414 7.294-1.06-1.06-5.798 5.772a3.102 3.102 0 0 0-.862 1.635l-.632 3.424a.778.778 0 0 0 .925.9l3.38-.694a3.124 3.124 0 0 0 1.574-.846l5.814-5.789-1.06-1.06-5.812 5.786a1.623 1.623 0 0 1-.819.44l-2.332.479.437-2.368c.059-.319.214-.614.445-.844l5.8-5.775Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 28a1 1 0 0 1-1.414 0L5 8.414A1 1 0 0 1 6.414 7L26 26.586A1 1 0 0 1 26 28Z m24.039 5.024 2.94 2.928a.065.065 0 0 1 0 .095l-8.368 8.333 1.413 1.415 8.367-8.331a2.065 2.065 0 0 0 0-2.929L25.45 3.607a2.086 2.086 0 0 0-2.941 0l-8.353 8.317 1.414 1.414 8.35-8.314a.074.074 0 0 1 .025-.018A.09.09 0 0 1 23.98 5c.014 0 .025.003.034.006a.075.075 0 0 1 .026.018Zm-9.886 9.726-1.414-1.415-7.73 7.697a4.136 4.136 0 0 0-1.15 2.18l-.841 4.565c-.134.723.51 1.35 1.233 1.201l4.505-.926a4.165 4.165 0 0 0 2.1-1.128l7.751-7.718-1.414-1.415-7.748 7.716c-.298.296-.677.5-1.092.586l-3.109.639.582-3.157c.079-.425.286-.818.595-1.126l7.732-7.7Z";
}