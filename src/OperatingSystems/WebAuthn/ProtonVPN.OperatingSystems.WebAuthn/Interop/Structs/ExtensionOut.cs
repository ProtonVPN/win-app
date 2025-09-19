using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about Extension.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_EXTENSION.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class ExtensionOut
{
    /// <summary>
    /// Extension identifier.
    /// </summary>
    public string Identifier { get; private set; }

    private int _dataLength;

    private ByteArrayOut _data;

    /// <summary>
    /// Extension data.
    /// </summary>
    public byte[] Data => _data?.Read(_dataLength);

    private ExtensionOut() { }
}
