namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

/// <summary>
/// Version of the WEBAUTHN_CLIENT_DATA structure, to allow for modifications in the future.
/// </summary>
public enum ClientDataVersion : uint
{
    /// <remarks>Corresponds to WEBAUTHN_CLIENT_DATA_CURRENT_VERSION.</remarks>
    Current = PInvoke.WEBAUTHN_CLIENT_DATA_CURRENT_VERSION
}
