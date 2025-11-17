# Microsoft Partner Center Signing

## Prerequisites
- Build the `ProtonVPN.CalloutDriver` project for both `x64` and `arm64` in the `Release` configuration.

## Signing workflow (x64)
1. Move the generated symbols from `bin\Release\x64\ProtonVPN.CalloutDriver.pdb` to `bin\Release\x64\ProtonVPN.CalloutDriver\ProtonVPN.CalloutDriver.pdb`.
2. Sign `bin\Release\x64\ProtonVPN.CalloutDriver\ProtonVPN.CalloutDriver.sys` with the Proton AG EV code-signing certificate and overwrite the unsigned file.
3. Create the cabinet package:

   ```
   makecab /f x64.ddf
   ```

4. Sign `ProtonVPN.CalloutDriver.x64.cab` with the Proton AG EV code-signing certificate.
5. Upload `ProtonVPN.CalloutDriver.x64.cab` to Microsoft Partner Center.
6. After Microsoft returns the signed package, download the updated files and replace:
   - `Setup\Native\x64\protonvpn.calloutdriver.cat`
   - `Setup\Native\x64\ProtonVPN.CalloutDriver.inf`
   - `Setup\Native\x64\ProtonVPN.CalloutDriver.sys`

## Signing workflow (arm64)
Repeat the steps above using the `arm64` build outputs, adjusting file names and paths as needed.