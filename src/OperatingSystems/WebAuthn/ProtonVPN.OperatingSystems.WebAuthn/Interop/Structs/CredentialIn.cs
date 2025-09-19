using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about credential.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_CREDENTIAL.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class CredentialIn : IDisposable
{
    /// <summary>
    /// Version of this structure, to allow for modifications in the future.
    /// </summary>
    private protected CredentialVersion Version = CredentialVersion.Current;

    private int _idLength;

    /// <summary>
    /// Unique ID for this particular credential.
    /// </summary>
    private ByteArrayIn _id;

    /// <summary>
    /// Well-known credential type specifying what this particular credential is.
    /// </summary>
    private string _type;

    public CredentialIn(byte[] id, string type)
    {
        _id = new ByteArrayIn(id);
        _idLength = id?.Length ?? 0;
        _type = type;
    }

    public void Dispose()
    {
        _id?.Dispose();
        _id = null;
    }
}
