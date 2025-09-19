namespace ProtonVPN.OperatingSystems.WebAuthn.Interop;

/// <summary>
/// Contains WebAuthn API constants.
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// WebAuthn operation type used when creating new FIDO2 credentials.
    /// </summary>
    public const string ClientDataCredentialCreate = "webauthn.create";

    /// <summary>
    /// WebAuthn operation type used when creating new FIDO U2F credentials.
    /// </summary>
    public const string ClientDataCredentialCreateU2F = "navigator.id.finishEnrollment";

    /// <summary>
    /// WebAuthn operation type used for authentication with FIDO2 credentials.
    /// </summary>
    public const string ClientDataCredentialGet = "webauthn.get";

    /// <summary>
    /// WebAuthn operation type used for authentication with FIDO U2F credentials.
    /// </summary>
    public const string ClientDataCredentialGetU2F = "navigator.id.getAssertion";

    /// <summary>
    /// Default timeut for WebAuthn operations.
    /// </summary>
    public const int DefaultTimeoutMilliseconds = 60000;

    /// <remarks>
    /// Corresponds to WEBAUTHN_CTAP_ONE_HMAC_SECRET_LENGTH.
    /// </remarks>
    public const uint CtapOneHmacSecretLength = PInvoke.WEBAUTHN_CTAP_ONE_HMAC_SECRET_LENGTH;

    /// <remarks>
    /// Corresponds to WEBAUTHN_CREDENTIAL_TYPE_PUBLIC_KEY.
    /// </remarks>
    public const string CredentialTypePublicKey = PInvoke.WEBAUTHN_CREDENTIAL_TYPE_PUBLIC_KEY;

    /// <remarks>
    /// Corresponds to WEBAUTHN_HASH_ALGORITHM_SHA_256.
    /// </remarks>
    public const string HashAlgorithmSha256 = PInvoke.WEBAUTHN_HASH_ALGORITHM_SHA_256;

    /// <remarks>
    /// Corresponds to WEBAUTHN_HASH_ALGORITHM_SHA_384.
    /// </remarks>
    public const string HashAlgorithmSha384 = PInvoke.WEBAUTHN_HASH_ALGORITHM_SHA_384;

    /// <remarks>
    /// Corresponds to WEBAUTHN_HASH_ALGORITHM_SHA_512.
    /// </remarks>
    public const string HashAlgorithmSha512 = PInvoke.WEBAUTHN_HASH_ALGORITHM_SHA_512;

    /// <remarks>
    /// Corresponds to WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_PROTECT.
    /// </remarks>
    public const string ExtensionIdentifierCredProtect = PInvoke.WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_PROTECT;

    /// <remarks>
    /// Corresponds to WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_BLOB.
    /// </remarks>
    public const string ExtensionIdentifierCredBlob = PInvoke.WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_BLOB;

    /// <remarks>
    /// Corresponds to WEBAUTHN_EXTENSIONS_IDENTIFIER_MIN_PIN_LENGTH.
    /// </remarks>
    public const string ExtensionIdentifierMinPinLength = PInvoke.WEBAUTHN_EXTENSIONS_IDENTIFIER_MIN_PIN_LENGTH;

    /// <remarks>
    /// Corresponds to WEBAUTHN_EXTENSIONS_IDENTIFIER_HMAC_SECRET.
    /// </remarks>
    public const string ExtensionIdentifierHmacSecret = PInvoke.WEBAUTHN_EXTENSIONS_IDENTIFIER_HMAC_SECRET;
}
