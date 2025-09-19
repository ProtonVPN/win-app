using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Contains the SALT values for the Hmac-Secret.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_HMAC_SECRET_SALT.</remarks>
[StructLayout(LayoutKind.Sequential)]
public class HmacSecretSaltIn : IDisposable
{
    /// <summary>
    /// Size of _first.
    /// </summary>
    private int _firstLength;

    /// <summary>
    /// The first SALT value.
    /// </summary>
    private ByteArrayIn _first;

    /// <summary>
    /// Size of _second.
    /// </summary>
    private int _secondLength;

    /// <summary>
    /// The second SALT value.
    /// </summary>
    private ByteArrayIn _second;

    public HmacSecretSaltIn(byte[] first, byte[] second)
    {
        // TODO: Check if the length of the values is WEBAUTHN_CTAP_ONE_HMAC_SECRET_LENGTH.
        _first = new ByteArrayIn(first);
        _firstLength = first?.Length ?? 0;

        _second = new ByteArrayIn(second);
        _secondLength = second?.Length ?? 0;
    }

    public void Dispose()
    {
        _first?.Dispose();
        _first = null;

        _second?.Dispose();
        _second = null;
    }
}
