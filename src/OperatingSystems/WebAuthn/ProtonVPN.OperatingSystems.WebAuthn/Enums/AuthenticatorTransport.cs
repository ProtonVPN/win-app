using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn.Enums;

/// <summary>
/// Defines hints as to how clients might communicate with a particular authenticator in order to obtain an assertion for a specific credential.
/// </summary>
/// <see>https://www.w3.org/TR/webauthn/#enum-transport</see>
[Flags]
[JsonConverter(typeof(JsonCustomEnumConverter<AuthenticatorTransport>))]
public enum AuthenticatorTransport : uint
{
    /// <summary>
    /// No transport restrictions.
    /// </summary>
    [EnumMember(Value = "NULL")]
#pragma warning disable CA1008 // Enums should have zero value
    NoRestrictions = 0,
#pragma warning restore CA1008 // Enums should have zero value

    /// <summary>
    /// Universal Serial Bus (USB).
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_CTAP_TRANSPORT_USB.</remarks>
    [EnumMember(Value = "usb")]
    USB = PInvoke.WEBAUTHN_CTAP_TRANSPORT_USB,

    /// <summary>
    /// Near Field Communication (NFC).
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_CTAP_TRANSPORT_NFC.</remarks>
    [EnumMember(Value = "nfc")]
    NFC = PInvoke.WEBAUTHN_CTAP_TRANSPORT_NFC,

    /// <summary>
    /// Bluetooth Low Energy (BLE).
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_CTAP_TRANSPORT_BLE.</remarks>
    [EnumMember(Value = "ble")]
    BLE = PInvoke.WEBAUTHN_CTAP_TRANSPORT_BLE,

    /// <summary>
    /// Hybrid Transport (QR Code).
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_CTAP_TRANSPORT_HYBRID.</remarks>
    [EnumMember(Value = "hybrid")]
    Hybrid = PInvoke.WEBAUTHN_CTAP_TRANSPORT_HYBRID,

    /// <remarks>Corresponds to WEBAUTHN_CTAP_TRANSPORT_TEST.</remarks>
    Test = PInvoke.WEBAUTHN_CTAP_TRANSPORT_TEST,

    /// <summary>
    /// Client device-specific transport.
    /// </summary>
    /// <remarks>Corresponds to WEBAUTHN_CTAP_TRANSPORT_INTERNAL.</remarks>
    [EnumMember(Value = "internal")]
    Internal = PInvoke.WEBAUTHN_CTAP_TRANSPORT_INTERNAL
}
