using ProtonVPN.Modals;

namespace ProtonVPN.About
{
    public class LicenseModalViewModel : BaseModalViewModel
    {
        public string License { get; } = @"## Copying

Copyright(c) 2023 Proton AG

Proton VPN is free software: you can redistribute it and / or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Proton VPN is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY;
without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Proton VPN. If not, see [https://www.gnu.org/licenses](https://www.gnu.org/licenses/).

## Dependencies

Proton VPN Windows app includes the following libraries:

* [ProtonMail SRP library](https://github.com/ProtonMail/go-srp) by Proton AG
  | [The MIT License](https://github.com/ProtonMail/go-srp/blob/master/LICENSE.txt).

Proton VPN Windows app includes the following 3rd party software:

* [OpenVPN client](https://github.com/OpenVPN/openvpn) by OpenVPN Inc.
  | [The GPL v2](https://github.com/OpenVPN/openvpn/blob/master/COPYRIGHT.GPL)
* [OpenVPN TAP - Windows adapter](https://github.com/OpenVPN/tap-windows6) by OpenVPN Technologies, Inc., Alon Bar-Lev
  | [The GPL v2](https://github.com/OpenVPN/tap-windows6/blob/master/COPYRIGHT.GPL)
* [Autofac](https://autofac.org/) by Autofac Contributors
  | [The MIT License](https://licenses.nuget.org/MIT).
* [Albireo.Base32](https://github.com/kappa7194/base32) by Albireo
  |  [The MIT License](https://github.com/kappa7194/base32/blob/master/LICENSE.txt).
* [ARSoft.Tools.Net](https://github.com/alexreinert/ARSoft.Tools.Net) By Alexander Reinert
  | [Apache License 2.0](https://github.com/alexreinert/ARSoft.Tools.Net/blob/master/LICENSE)
* [ByteSize](https://github.com/omar/ByteSize) by Omar Khudeira
  | [The MIT License](https://raw.githubusercontent.com/omar/ByteSize/master/LICENSE).
* [CalcBinding](https://github.com/Alex141/CalcBinding) by Alexander Zinchenko
  | [Apache License 2.0](https://www.nuget.org/packages/CalcBinding/2.5.2/license).
* [Caliburn.Micro](https://caliburnmicro.com/) by Rob Eisenberg, Marco Amendola,
  Chin Bae, Ryan Cromwell, Nigel Sampson, Thomas Ibel, Matt Hidinger
  | [The MIT License](https://raw.githubusercontent.com/Caliburn-Micro/Caliburn.Micro/master/License.txt).
* [Caliburn.Micro.Core](https://caliburnmicro.com/) by Rob Eisenberg, Marco Amendola,
  Chin Bae, Ryan Cromwell, Nigel Sampson, Thomas Ibel, Matt Hidinger
  | [The MIT License](https://raw.githubusercontent.com/Caliburn-Micro/Caliburn.Micro/master/License.txt).
* [DeviceId](https://github.com/MatthewKing/DeviceId) by Matthew King
  | [The MIT License](https://github.com/MatthewKing/DeviceId/blob/main/license.txt).
* [DnsClient](http://dnsclient.michaco.net/) by MichaCo
  | [Apache License 2.0](https://github.com/MichaCo/DnsClient.NET/blob/master/LICENSE).
* [DynamicExpresso.Core](https://github.com/davideicardi/DynamicExpresso) by Davide Icardi
  | [The MIT License](https://github.com/davideicardi/DynamicExpresso/blob/master/LICENSE).
* [MvvmLightLibsStd10](https://github.com/lbugnion/mvvmlight) by Laurent Bugnion (GalaSoft)
  | [The MIT License](https://github.com/lbugnion/mvvmlight/blob/master/LICENSE). 
* [Newtonsoft.Json](https://www.newtonsoft.com/json) by James Newton-King
  | [The MIT License](https://licenses.nuget.org/MIT).
* [Apache log4net](https://logging.apache.org/log4net/) by The Apache Software Foundation
  | [Apache License 2.0](https://logging.apache.org/log4net/license.html).
* [OxyPlot.Core](https://github.com/oxyplot/oxyplot/) by Oystein Bjorke
  | [The MIT License](https://raw.githubusercontent.com/oxyplot/oxyplot/master/LICENSE).
* [OxyPlot.Wpf](https://github.com/oxyplot/oxyplot/) by Oystein Bjorke
  | [The MIT License](https://raw.githubusercontent.com/oxyplot/oxyplot/master/LICENSE).
* [PInvoke.Kernel32](https://github.com/AArnott/pinvoke) by Andrew Arnott
  | [The MIT License](https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE).
* [PInvoke.Windows.Core](https://github.com/AArnott/pinvoke) by Andrew Arnott
  | [The MIT License](https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE).
* [PInvoke.Windows.ShellScalingApi](https://github.com/AArnott/pinvoke) by Andrew Arnott
  | [The MIT License](https://raw.githubusercontent.com/AArnott/pinvoke/cf0176c42b/LICENSE).
* [Polly](https://github.com/App-vNext/Polly) by Michael Wolfenden, App vNext
  | [The 3 - Clause BSD License](https://opensource.org/licenses/BSD-3-Clause).
* [Polly.Contrib.WaitAndRetry](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry) by Grant Dickinson
  | [The 3 - Clause BSD License](https://opensource.org/licenses/BSD-3-Clause).
* [PluralNet](https://github.com/rudyhuyn/PluralNet) by Rudy Huyn
  | [The MIT License](https://github.com/rudyhuyn/PluralNet/blob/master/LICENSE).
* [Sentry](https://sentry.io) by Sentry Team and Contributors
  | [The MIT License](https://raw.githubusercontent.com/getsentry/sentry-dotnet/master/LICENSE).
* [Sentry.PlatformAbstractions](https://sentry.io) by Sentry Team and Contributors
  | [The MIT License](https://github.com/getsentry/sentry-dotnet-platform-abstractions/blob/master/LICENSE).
* [Sentry.Protocol](https://sentry.io) by Sentry Team and Contributors
  | [The MIT License](https://raw.githubusercontent.com/getsentry/sentry-dotnet-protocol/master/LICENSE).
* [System.Buffers](https://dot.net) by Microsoft
  | [The MIT License](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT).
* [System.Collections.Immutable](https://dot.net) by Microsoft
  | [The MIT License](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT).";
    }
}