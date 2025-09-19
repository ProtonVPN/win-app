using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs.PreviousVersions;

/// <summary>
/// This structure comes from an older version of the API and should only be used by Marshal.Sizeof().
/// </summary>
/// <see cref="GetAssertion.Assertion"/>
/// <remarks>Corresponds to WEBAUTHN_ASSERTION.</remarks>
[StructLayout(LayoutKind.Sequential)]
public sealed class AssertionV2
{
    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private AssertionVersion _version;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private int _authenticatorDataLength;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private ByteArrayOut _authenticatorData;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private int _signatureLength;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private ByteArrayOut _signature;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private CredentialOut _credential;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private int _userIdLength;

    /// <remarks>This field has been present since WEBAUTHN_ASSERTION_VERSION_1.</remarks>
    private ByteArrayOut _userId;

    /// <remarks>This field has been added in WEBAUTHN_ASSERTION_VERSION_2.</remarks>
    private ExtensionsOut _extensions;

    /// <remarks>This field has been added in WEBAUTHN_ASSERTION_VERSION_2.</remarks>
    private int _largeBlobLength;

    /// <remarks>This field has been added in WEBAUTHN_ASSERTION_VERSION_2.</remarks>
    private ByteArrayOut _largeBlob;

    /// <remarks>This field has been added in WEBAUTHN_ASSERTION_VERSION_2.</remarks>
    private CredentialLargeBlobStatus _largeBlobStatus;

    /// <summary>
    /// The instantiation of this class is blocked by this private constructor.
    /// </summary>
    private AssertionV2() { }
}
