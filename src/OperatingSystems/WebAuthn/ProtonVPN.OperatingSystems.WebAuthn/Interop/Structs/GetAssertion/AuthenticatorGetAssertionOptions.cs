using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs.GetAssertion;

/// <summary>
/// A structure that contains the data needed to get an assertion.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class AuthenticatorGetAssertionOptions : IDisposable
{
    /// <summary>
    /// The version of this structure.
    /// </summary>
    private AuthenticatorGetAssertionOptionsVersion _version = AuthenticatorGetAssertionOptionsVersion.Version7;

    /// <summary>
    /// Time that the operation is expected to complete within.
    /// </summary>
    /// <remarks>This is used as guidance, and can be overridden by the platform.</remarks>
    public int TimeoutMilliseconds { get; set; } = ApiConstants.DefaultTimeoutMilliseconds;

    /// <summary>
    /// The list of allowed credentials to be used in the assertion.
    /// </summary>
    private Credentials _allowCredentials;

    /// <summary>
    /// A CBOR map from extension identifiers to their authenticator extension inputs,
    /// created by the client based on the extensions requested by the Relying Party.
    /// These are optional extensions to parse when performing the operation.
    /// </summary>
    private ExtensionsIn _extensions;

    /// <summary>
    /// Platform vs Cross-Platform Authenticators. (Optional)
    /// </summary>
    public AuthenticatorAttachment AuthenticatorAttachment { get; set; }

    /// <summary>
    /// The effective user verification requirement.
    /// </summary>
    public UserVerificationRequirement UserVerificationRequirement { get; set; }

    /// <summary>
    /// Flags
    /// </summary>
    private AssertionOptionsFlags _flags;

    /// <summary>
    /// Optional identifier for the U2F AppId. Converted to UTF8 before being hashed. Not lower-cased.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_2.</remarks>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    private string _u2fAppId;

    /// <summary>
    /// If the following is non-NULL, then, set to TRUE if the above U2fAppid was used instead of RpId
    /// Note that this value is modified by WebAuthNAuthenticatorGetAssertion
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_2.</remarks>
    private nint _isU2fAppIdUsed = nint.Zero;

    /// <summary>
    /// Cancellation Id (Optional).
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_3.</remarks>
    private nint _cancellationId = nint.Zero;

    /// <summary>
    /// An optional list of public key credential descriptors describing credentials acceptable to the Relying Party (possibly filtered by the client), if any.
    /// If present, CredentialList will be ignored.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_4.</remarks>
    private nint _allowCredentialList = nint.Zero;

    /// <summary>
    /// The large blob operation.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_5.</remarks>
    public CredentialLargeBlobOperation LargeBlobOperation { get; set; }

    /// <summary>
    /// Size of _largeBlob.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_5.</remarks>
    private int _largeBlobLength;

    /// <summary>
    /// A pointer to the large credential blob.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_5.</remarks>
    private ByteArrayIn _largeBlob;

    /// <summary>
    /// PRF values which will be converted into HMAC-SECRET values according to WebAuthn Specification.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_6.</remarks>
    private nint _hmacSecretSaltValues;

    /// <summary>
    /// Indicates whether the browser is in private mode. Defaulting to false.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_6.</remarks>
    public bool BrowserInPrivateMode { get; set; }

    /// <summary>
    /// Linked Device Connection Info.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_7.</remarks>
    private nint _linkedDevice { get; set; }

    /// <summary>
    /// Allowlist MUST contain 1 credential applicable for Hybrid transport.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_7.</remarks>
    bool AutoFill { get; set; }

    /// <summary>
    /// Size of JSON extension.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_7.</remarks>
    private int _jsonExtLength;

    /// <summary>
    /// JSON extension.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_7.</remarks>
    private ByteArrayIn _jsonExt;


    /// <summary>
    /// Optional identifier for the U2F AppId. Converted to UTF8 before being hashed. Not lower cased.
    /// </summary>
    public string U2fAppId
    {
        get
        {
            return _u2fAppId;
        }
        set
        {
            _u2fAppId = value;
            if (_isU2fAppIdUsed == nint.Zero)
            {
                _isU2fAppIdUsed = Marshal.AllocHGlobal(sizeof(int));
            }

            Marshal.WriteInt32(_isU2fAppIdUsed, Convert.ToInt32(value != null));
        }
    }

    /// <summary>
    /// Cancellation Id (Optional)
    /// </summary>
    public Guid? CancellationId
    {
        set
        {
            if (value.HasValue)
            {
                if (_cancellationId == nint.Zero)
                {
                    _cancellationId = Marshal.AllocHGlobal(Marshal.SizeOf<Guid>());
                }

                Marshal.StructureToPtr(value.Value, _cancellationId, false);
            }
            else
            {
                FreeCancellationId();
            }
        }
    }

    /// <summary>
    /// Allowed Credentials List.
    /// </summary>
    public Credentials AllowCredentials
    {
        set
        {
            _allowCredentials?.Dispose();
            _allowCredentials = value;
        }
    }

    /// <summary>
    /// Allow Credential List. If present, "AllowCredentials" will be ignored.
    /// </summary>
    public CredentialList AllowCredentialsEx
    {
        set
        {
            if (value != null)
            {
                if (_allowCredentialList == nint.Zero)
                {
                    _allowCredentialList = Marshal.AllocHGlobal(Marshal.SizeOf<CredentialList>());
                }

                Marshal.StructureToPtr(value, _allowCredentialList, false);
            }
            else
            {
                FreeAllowCredentialList();
            }
        }
    }

    /// <summary>
    /// Version of this structure, to allow for modifications in the future.
    /// </summary>
    /// <remarks>This is a V7 struct. If V8 arrives, new fields will need to be added.</remarks>
    public AuthenticatorGetAssertionOptionsVersion Version
    {
        get
        {
            return _version;
        }
        set
        {
            if (value > AuthenticatorGetAssertionOptionsVersion.Version7)
            {
                // We only support older struct versions.
                throw new ArgumentOutOfRangeException(nameof(value), "The requested data structure version is not yet supported.");
            }

            _version = value;
        }
    }

    /// <summary>
    /// Extensions to parse when performing the operation. (Optional)
    /// </summary>
    public ExtensionsIn Extensions
    {
        set
        {
            _extensions?.Dispose();
            _extensions = value;
        }
    }

    /// <summary>
    /// Credential Large Blob.
    /// </summary>
    public byte[] LargeBlob
    {
        get
        {
            return _largeBlob?.Read(_largeBlobLength);
        }
        set
        {
            // Get rid of any previous blob first
            _largeBlob?.Dispose();

            // Now replace the previous value with the new one
            _largeBlobLength = value?.Length ?? 0;
            _largeBlob = new ByteArrayIn(value);
        }
    }

    /// <summary>
    /// PRF values which will be converted into HMAC-SECRET values according to WebAuthn Specification.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_6.</remarks>

    public HmacSecretSaltValuesIn HmacSecretSaltValues
    {
        set
        {
            if (value?.HasGlobalHmacSalt ?? false)
            {
                if (_hmacSecretSaltValues == nint.Zero)
                {
                    _hmacSecretSaltValues = Marshal.AllocHGlobal(Marshal.SizeOf<HmacSecretSaltValuesIn>());
                }

                Marshal.StructureToPtr(value, _hmacSecretSaltValues, false);

                // Set flag
                _flags |= AssertionOptionsFlags.AuthenticatorHmacSecretValues;
            }
            else
            {
                FreeHmacSecretSaltValues();

                // Unset flag
                _flags &= ~AssertionOptionsFlags.AuthenticatorHmacSecretValues;
            }
        }
    }

    /// <summary>
    /// Linked Device Connection Info.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_7.</remarks>

    public HybridStorageLinkedData LinkedDevice
    {
        set
        {
            if (value != null)
            {
                if (_linkedDevice == nint.Zero)
                {
                    _linkedDevice = Marshal.AllocHGlobal(Marshal.SizeOf<HybridStorageLinkedData>());
                }

                Marshal.StructureToPtr(value, _linkedDevice, false);
            }
            else
            {
                FreeLinkedDevice();
            }
        }
    }

    /// <summary>
    /// JSON extension.
    /// </summary>
    /// <remarks>This field has been added in WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_7.</remarks>
    public byte[] JsonExt
    {
        get
        {
            return _jsonExt?.Read(_jsonExtLength);
        }
        set
        {
            // Get rid of any previous blob first
            _jsonExt?.Dispose();

            // Now replace the previous value with a new one
            _jsonExtLength = value?.Length ?? 0;
            _jsonExt = new ByteArrayIn(value);
        }
    }

    public void Dispose()
    {
        _extensions?.Dispose();
        _extensions = null;

        _allowCredentials?.Dispose();
        _allowCredentials = null;

        _largeBlob?.Dispose();
        _largeBlob = null;

        _jsonExt?.Dispose();
        _jsonExt = null;

        FreeAllowCredentialList();

        FreeHmacSecretSaltValues();

        FreeLinkedDevice();

        if (_isU2fAppIdUsed != nint.Zero)
        {
            Marshal.FreeHGlobal(_isU2fAppIdUsed);
            _isU2fAppIdUsed = nint.Zero;
        }

        FreeCancellationId();
    }

    private void FreeLinkedDevice()
    {
        if (_linkedDevice != nint.Zero)
        {
            Marshal.FreeHGlobal(_linkedDevice);
            _linkedDevice = nint.Zero;
        }
    }

    private void FreeHmacSecretSaltValues()
    {
        if (_hmacSecretSaltValues != nint.Zero)
        {
            Marshal.FreeHGlobal(_hmacSecretSaltValues);
            _hmacSecretSaltValues = nint.Zero;
        }
    }

    private void FreeAllowCredentialList()
    {
        if (_allowCredentialList != nint.Zero)
        {
            Marshal.FreeHGlobal(_allowCredentialList);
            _allowCredentialList = nint.Zero;
        }
    }

    private void FreeCancellationId()
    {
        if (_cancellationId != nint.Zero)
        {
            Marshal.FreeHGlobal(_cancellationId);
            _cancellationId = nint.Zero;
        }
    }
}
