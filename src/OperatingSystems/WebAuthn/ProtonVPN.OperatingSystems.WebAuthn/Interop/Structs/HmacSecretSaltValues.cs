using System.Runtime.InteropServices;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// The structure that contains the SALT values for the HMAC secret.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_HMAC_SECRET_SALT_VALUES.</remarks>
[StructLayout(LayoutKind.Sequential)]
public class HmacSecretSaltValuesIn : IDisposable
{
    /// <summary>
    /// The global HMAC SALT.
    /// </summary>
    private nint _globalHmacSalt;

    /// <summary>
    /// List of credentials with HMAC secret SALT.
    /// </summary>
    private CredentialsWithHmacSecretSaltIn _credWithHmacSecretSaltList;

    public HmacSecretSaltValuesIn(HmacSecretSaltIn globalHmacSalt, CredentialWithHmacSecretSaltIn[] credsWithHmacSecretSalt)
    {
        if (globalHmacSalt != null)
        {
            _globalHmacSalt = Marshal.AllocHGlobal(Marshal.SizeOf<HmacSecretSaltIn>());
            Marshal.StructureToPtr(globalHmacSalt, _globalHmacSalt, false);
        }

        _credWithHmacSecretSaltList = new CredentialsWithHmacSecretSaltIn(credsWithHmacSecretSalt);
    }

    public bool HasGlobalHmacSalt => _globalHmacSalt != nint.Zero;

    public void Dispose()
    {
        if (_globalHmacSalt != nint.Zero)
        {
            Marshal.FreeHGlobal(_globalHmacSalt);
            _globalHmacSalt = nint.Zero;
        }

        _credWithHmacSecretSaltList?.Dispose();
        _credWithHmacSecretSaltList = null;
    }
}
