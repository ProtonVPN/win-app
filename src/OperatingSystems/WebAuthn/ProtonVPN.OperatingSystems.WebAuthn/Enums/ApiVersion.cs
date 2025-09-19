namespace ProtonVPN.OperatingSystems.WebAuthn.Enums;

/// <summary>
/// WebAuthn API Version Information.
/// </summary>
#pragma warning disable CA1027 // Mark enums with FlagsAttribute
#pragma warning disable CA1008 // Enums should have zero value
public enum ApiVersion : uint
#pragma warning restore CA1027 // Mark enums with FlagsAttribute
#pragma warning restore CA1008 // Enums should have zero value
{
    /// <summary>
    /// Baseline Version
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_1.</remarks>
    Version1 = PInvoke.WEBAUTHN_API_VERSION_1,

    /// <summary>
    /// Delta From V1.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_2.</remarks>
    Version2 = PInvoke.WEBAUTHN_API_VERSION_2,

    /// <summary>
    /// Delta From V2.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_3.</remarks>
    Version3 = PInvoke.WEBAUTHN_API_VERSION_3,

    /// <summary>
    /// Delta From V3.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_4.</remarks>
    Version4 = PInvoke.WEBAUTHN_API_VERSION_4,

    /// <summary>
    /// Delta From V4.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_5.</remarks>
    Version5 = PInvoke.WEBAUTHN_API_VERSION_5,

    /// <summary>
    /// Delta From V5.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_6.</remarks>
    Version6 = PInvoke.WEBAUTHN_API_VERSION_6,

    /// <summary>
    /// Delta From V6.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_VERSION_6.</remarks>
    Version7 = PInvoke.WEBAUTHN_API_VERSION_7,

    /// <summary>
    /// Current Version
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_API_CURRENT_VERSION.</remarks>
#pragma warning disable CA1069 // Enums values should not be duplicated
    Current = PInvoke.WEBAUTHN_API_CURRENT_VERSION
#pragma warning restore CA1069 // Enums values should not be duplicated
}
