using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about credential list with extra information.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_CREDENTIALS.</remarks>
[StructLayout(LayoutKind.Sequential)]
public sealed class Credentials : SafeStructArrayIn<CredentialIn>
{
    public Credentials(CredentialIn[] credentials) : base(credentials) { }
}
