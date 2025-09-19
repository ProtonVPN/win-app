namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

/// <summary>
/// The version of the WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS structure, to allow for modifications in the future.
/// </summary>
public enum AuthenticatorGetAssertionOptionsVersion : uint
{
    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_1.
    /// </remarks>
    Version1 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_1,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_2.
    /// </remarks>
    Version2 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_2,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_3.
    /// </remarks>
    Version3 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_3,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_4.
    /// </remarks>
    Version4 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_4,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_5.
    /// </remarks>
    Version5 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_5,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_6.
    /// </remarks>
    Version6 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_6,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_7.
    /// </remarks>
    Version7 = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_7,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_CURRENT_VERSION.
    /// </remarks>
#pragma warning disable CA1069 // Enums values should not be duplicated
    Current = PInvoke.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_CURRENT_VERSION
#pragma warning restore CA1069 // Enums values should not be duplicated
}
