using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about credential with extra information, such as, Transports.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_CREDENTIAL_EX.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class CredentialEx : IDisposable
{
    /// <summary>
    /// Version of this structure, to allow for modifications in the future.
    /// </summary>
    private CredentialExVersion Version { get; set; } = CredentialExVersion.Current;

    private int _idLength;

    /// <summary>
    /// Unique ID for this particular credential.
    /// </summary>
    private ByteArrayIn _id;

    /// <summary>
    /// Well-known credential type specifying what this particular credential is.
    /// </summary>
    private string _type;

    /// <summary>
    /// Transports.
    /// </summary>
    private AuthenticatorTransport _transport;

    public CredentialEx(byte[] id, string type, AuthenticatorTransport transport)
    {
        _id = new ByteArrayIn(id);
        _idLength = id?.Length ?? 0;
        _type = type;
        _transport = transport;
    }

    public void Dispose()
    {
        _id?.Dispose();
        _id = null;
    }
}
