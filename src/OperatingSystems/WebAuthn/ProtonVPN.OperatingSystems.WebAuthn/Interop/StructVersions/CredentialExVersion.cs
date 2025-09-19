namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

/// <summary>
/// The current version of the WEBAUTHN_CREDENTIAL_EX structure, to allow for modifications in the future.
/// </summary>
public enum CredentialExVersion : uint
{
    /// <remarks>Corresponds to WEBAUTHN_CREDENTIAL_EX_CURRENT_VERSION.</remarks>
    Current = PInvoke.WEBAUTHN_CREDENTIAL_EX_CURRENT_VERSION
}
