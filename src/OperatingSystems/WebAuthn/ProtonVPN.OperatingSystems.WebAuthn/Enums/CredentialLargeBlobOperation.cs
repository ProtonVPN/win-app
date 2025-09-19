namespace ProtonVPN.OperatingSystems.WebAuthn.Enums;

/// <summary>
/// The operation to perform on the large blob.
/// </summary>
public enum CredentialLargeBlobOperation : uint
{
    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_OPERATION_NONE.
    /// </remarks>
    None = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_OPERATION_NONE,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_OPERATION_GET.
    /// </remarks>
    Get = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_OPERATION_GET,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_OPERATION_SET.
    /// </remarks>
    Set = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_OPERATION_SET,

    /// <remarks>
    /// Corresponds to WEBAUTHN_CRED_LARGE_BLOB_OPERATION_DELETE.
    /// </remarks>
    Delete = PInvoke.WEBAUTHN_CRED_LARGE_BLOB_OPERATION_DELETE
}
