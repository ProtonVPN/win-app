using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <remarks>Corresponds to WEBAUTHN_CRED_WITH_HMAC_SECRET_SALT.</remarks>
[StructLayout(LayoutKind.Sequential)]
public class CredentialWithHmacSecretSaltIn : IDisposable
{
    /// <summary>
    /// Size of the credential ID.
    /// </summary>
    private int _credentialIdLength;

    /// <summary>
    /// Credential ID.
    /// </summary>
    private ByteArrayIn _credentialId;

    /// <summary>
    /// PRF Values for above credential
    /// </summary>
    private nint _hmacSecretSalt;

    /// <summary>
    /// Credential ID.
    /// </summary>
    public byte[] CredentialId
    {
        get
        {
            return _credentialId?.Read(_credentialIdLength);
        }
        set
        {
            // Get rid of any previous value first
            _credentialId?.Dispose();

            // Now replace the previous value with a new one
            _credentialIdLength = value?.Length ?? 0;
            _credentialId = new ByteArrayIn(value);
        }
    }

    /// <summary>
    /// PRF Values for above credential
    /// </summary>
    public HmacSecretSaltIn HmacSecretSalt
    {
        set
        {
            if (value != null)
            {
                if (_hmacSecretSalt == nint.Zero)
                {
                    _hmacSecretSalt = Marshal.AllocHGlobal(Marshal.SizeOf<HmacSecretSaltIn>());
                }

                Marshal.StructureToPtr(value, _hmacSecretSalt, false);
            }
            else
            {
                FreeHmacSecretSalt();
            }
        }
    }

    public CredentialWithHmacSecretSaltIn(byte[] credentialId, HmacSecretSaltIn hmacSecretSalt)
    {
        CredentialId = credentialId;
        HmacSecretSalt = hmacSecretSalt;
    }

    private void FreeHmacSecretSalt()
    {
        if (_hmacSecretSalt != nint.Zero)
        {
            Marshal.FreeHGlobal(_hmacSecretSalt);
            _hmacSecretSalt = nint.Zero;
        }
    }

    public void Dispose()
    {
        _credentialId?.Dispose();
        _credentialId = null;

        FreeHmacSecretSalt();
    }
}

[StructLayout(LayoutKind.Sequential)]
public sealed class CredentialsWithHmacSecretSaltIn : SafeStructArrayIn<CredentialWithHmacSecretSaltIn>
{
    public CredentialsWithHmacSecretSaltIn(CredentialWithHmacSecretSaltIn[] credsWithHmacSecretSalt) : base(credsWithHmacSecretSalt)
    {
    }
}
