namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Enums;

[Flags]
public enum AssertionOptionsFlags : uint
{
    None = 0,
    AuthenticatorHmacSecretValues = PInvoke.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG
}
