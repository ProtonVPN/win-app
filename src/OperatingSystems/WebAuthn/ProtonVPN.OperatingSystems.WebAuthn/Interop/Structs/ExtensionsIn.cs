using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about Extensions.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_EXTENSIONS.</remarks>
[StructLayout(LayoutKind.Sequential)]
public class ExtensionsIn : SafeStructArrayIn<ExtensionIn>
{
    public ExtensionsIn(ExtensionIn[] extensions) : base(extensions)
    {
    }
}
