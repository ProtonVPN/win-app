using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Contains the SALT values for the Hmac-Secret.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_HMAC_SECRET_SALT.</remarks>
[StructLayout(LayoutKind.Sequential)]
public class HmacSecretSaltOut
{
    /// <summary>
    /// Size of _first.
    /// </summary>
    private int _firstLength;

    /// <summary>
    /// The first SALT value.
    /// </summary>
    private ByteArrayOut _first;

    /// <summary>
    /// Size of _second.
    /// </summary>
    private int _secondLength;

    /// <summary>
    /// The second SALT value.
    /// </summary>
    private ByteArrayOut _second;

    /// <summary>
    /// The first SALT value.
    /// </summary>
    public byte[] First => _first?.Read(_firstLength);

    /// <summary>
    /// The second SALT value.
    /// </summary>
    public byte[] Second => _second?.Read(_secondLength);

    private HmacSecretSaltOut() { }
}