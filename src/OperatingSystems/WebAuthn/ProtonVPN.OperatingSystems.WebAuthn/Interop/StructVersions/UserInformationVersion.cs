namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

/// <summary>
/// User Information Structure Version Information.
/// </summary>
public enum UserInformationVersion : uint
{
    /// <summary>
    /// Current version
    /// </summary>
    /// <remarks>
    /// Corresponds to WEBAUTHN_USER_ENTITY_INFORMATION_CURRENT_VERSION.
    /// </remarks>
    Current = PInvoke.WEBAUTHN_USER_ENTITY_INFORMATION_CURRENT_VERSION
}
