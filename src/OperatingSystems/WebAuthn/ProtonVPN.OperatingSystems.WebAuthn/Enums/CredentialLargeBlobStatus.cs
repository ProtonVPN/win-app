namespace ProtonVPN.OperatingSystems.WebAuthn.Enums;

/// <summary>
/// The status of the large blob operation.
/// </summary>
public enum CredentialLargeBlobStatus : uint
{
    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_NONE.
    /// </remarks>
    None = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_NONE,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_SUCCESS.
    /// </remarks>
    Success = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_SUCCESS,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_NOT_SUPPORTED.
    /// </remarks>
    NotSupported = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_NOT_SUPPORTED,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_INVALID_DATA.
    /// </remarks>
    InvalidData = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_INVALID_DATA,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_INVALID_PARAMETER.
    /// </remarks>
    InvalidParameter = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_INVALID_PARAMETER,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_NOT_FOUND.
    /// </remarks>
    NotFound = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_NOT_FOUND,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_MULTIPLE_CREDENTIALS.
    /// </remarks>
    MultipleCredentials = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_MULTIPLE_CREDENTIALS,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_LACK_OF_SPACE.
    /// </remarks>
    LackOfSpace = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_LACK_OF_SPACE,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_PLATFORM_ERROR.
    /// </remarks>
    PlatformError = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_PLATFORM_ERROR,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_STATUS_AUTHENTICATOR_ERROR.
    /// </remarks>
    AuthenticatorError = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_STATUS_AUTHENTICATOR_ERROR
}
