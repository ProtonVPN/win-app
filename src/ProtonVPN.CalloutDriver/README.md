## Release

### x64  

* Make sure Sign mode is set to **off** in project's settings (Driver Signing -> General -> Sign mode)
* Compile x64 release build
* copy file `src\ProtonVPN.CalloutDriver\bin\Release\x64\ProtonVPN.CalloutDriver.pdb` to  `src\ProtonVPN.CalloutDriver\bin\Release\x64\ProtonVPN.CalloutDriver\ProtonVPN.CalloutDriver.pdb` 
* `Sign src\ProtonVPN.CalloutDriver\bin\Release\x64\ProtonVPN.CalloutDriver\ProtonVPN.CalloutDriver.sys` with EV certificate and replace this file with a signed version of it.
* Create cab file (run this command from cmd at src\ProtonVPN.CalloutDriver) ```MakeCab /f .\ProtonVPN.CalloutDriver.x64.ddf```
* Sign `disk1\ProtonVPN.CalloutDriver.x64.cab` with EV certificate.
* Upload `ProtonVPN.CalloutDriver.x64.cab` to Microsoft Partner Center https://partner.microsoft.com/en-us/dashboard/hardware/driver/New

### arm64

The steps for building arm64 are identical to the x64 release process, except for substituting x64 with arm64.