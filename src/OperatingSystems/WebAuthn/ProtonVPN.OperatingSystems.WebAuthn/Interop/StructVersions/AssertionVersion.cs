namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

/// <summary>
/// The version of the WEBAUTHN_ASSERTION structure, to allow for modifications in the future.
/// </summary>
public enum AssertionVersion : uint
{
    /// <remarks>
    /// Corresponds to WEBAUTHN_ASSERTION_VERSION_1.
    /// </remarks>
    Version1 = PInvoke.WEBAUTHN_ASSERTION_VERSION_1,

    /// <remarks>
    /// Corresponds to WEBAUTHN_ASSERTION_VERSION_2.
    /// </remarks>
    Version2 = PInvoke.WEBAUTHN_ASSERTION_VERSION_2,

    /// <remarks>
    /// Corresponds to WEBAUTHN_ASSERTION_VERSION_3.
    /// </remarks>
    Version3 = PInvoke.WEBAUTHN_ASSERTION_VERSION_3,

    /// <remarks>
    /// Corresponds to WEBAUTHN_ASSERTION_VERSION_4.
    /// </remarks>
    Version4 = PInvoke.WEBAUTHN_ASSERTION_VERSION_4,

    /// <remarks>
    /// Corresponds to WEBAUTHN_ASSERTION_VERSION_5.
    /// </remarks>
    Version5 = PInvoke.WEBAUTHN_ASSERTION_VERSION_5,

    /// <remarks>
    /// Corresponds to WEBAUTHN_ASSERTION_CURRENT_VERSION.
    /// </remarks>
#pragma warning disable CA1069 // Enums values should not be duplicated
    Current = PInvoke.WEBAUTHN_ASSERTION_CURRENT_VERSION
#pragma warning restore CA1069 // Enums values should not be duplicated
}
