using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.FIDO;
using ProtonVPN.OperatingSystems.WebAuthn.Interop;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs.GetAssertion;

namespace ProtonVPN.OperatingSystems.WebAuthn;

/// <summary>
/// Windows WebAuthn API
/// </summary>
/// <remarks>
/// Requires Windows 10 1903+ to work.
/// </remarks>
public partial class WebAuthnApi
{
    private static ApiVersion? _apiVersionCache;
    private Guid? _cancellationId;

    /// <summary>
    /// Gets the API version information.
    /// </summary>
    /// <remarks>
    /// Indicates the presence of APIs and features.
    /// </remarks>
    public static ApiVersion? ApiVersion
    {
        get
        {
            try
            {
                return NativeMethods.GetApiVersionNumber();
            }
            catch (TypeLoadException)
            {
                // The WebAuthNGetApiVersionNumber() function was added in Windows 10 1903.
                return null;
            }
        }
    }

    /// <summary>
    /// Indicates the availability of the WebAuthn API.
    /// </summary>
    public static bool IsAvailable => ApiVersion >= Enums.ApiVersion.Version1;

    /// <summary>
    /// Indicates the availability of the Credential Protection extension.
    /// </summary>
    /// <remarks>
    /// Support for the credProtect extension was added in V2 API.
    /// </remarks>
    public static bool IsCredProtectExtensionSupported => ApiVersion >= Enums.ApiVersion.Version2;

    /// <summary>
    /// Indicates the availability of enterprise attestation.
    /// </summary>
    /// <remarks>
    /// Support for the enterprise attestation was added in V3 API.
    /// </remarks>
    public static bool IsEnterpriseAttestationSupported => ApiVersion >= Enums.ApiVersion.Version3;

    /// <summary>
    /// Indicates the availability of the Credential Blob extension.
    /// </summary>
    /// <remarks>
    /// Support for the credBlob extension was added in V3 API.
    /// </remarks>
    public static bool IsCredBlobSupported => ApiVersion >= Enums.ApiVersion.Version3;

    /// <summary>
    /// Indicates the availability of the large blobs.
    /// </summary>
    /// <remarks>
    /// Support for the large blobs was added in V5 API.
    /// </remarks>
    public static bool IsLargeBlobSupported => ApiVersion >= Enums.ApiVersion.Version5;

    /// <summary>
    /// Indicates the API can differentiate between browser modes.
    /// </summary>
    /// <remarks>
    /// Support for the browser mode indicator was added in V5 API.
    /// </remarks>
    public static bool IsPrivateBrowserModeIndicatorSupported => ApiVersion >= Enums.ApiVersion.Version5;

    /// <summary>
    /// Indicates the availability of the API for platform credential management.
    /// </summary>
    /// <remarks>
    /// Support for platform credential management was added in V4 API.
    /// </remarks>
    public static bool IsPlatformCredentialManagementSupported => ApiVersion >= Enums.ApiVersion.Version4;

    /// <summary>
    /// Indicates the availability of the minimum PIN length extension.
    /// </summary>
    /// <remarks>
    /// Support for the minPinLength extension was added in V3 API.
    /// </remarks>
    public static bool IsMinPinLengthSupported => ApiVersion >= Enums.ApiVersion.Version3;

    /// <summary>
    /// Indicates the availability of the psuedo-random function (PRF) extension.
    /// </summary>
    /// <remarks>
    /// Support for the prf extension was added in V6 API.
    /// </remarks>
    public static bool IsPseudoRandomFunctionSupported => ApiVersion >= Enums.ApiVersion.Version6;

    /// <summary>
    /// Indicates whether operation cancellation is supported by the API.
    /// </summary>
    public bool IsCancellationSupported => _cancellationId.HasValue;

    /// <summary>
    /// Indicates the support for unsigned extension outputs.
    /// </summary>
    /// <remarks>
    /// Support for the unsigned extension outputs was added in V7 API.
    /// </remarks>
    public static bool IsUnsignedExtensionOutputSupported => ApiVersion >= Enums.ApiVersion.Version7;

    /// <summary>
    /// Indicates the support for linked device data.
    /// </summary>
    /// <remarks>
    /// Support for linked device data was added in V7 API.
    /// </remarks>
    public static bool IsHybridStorageLinkedDataSupported => ApiVersion >= Enums.ApiVersion.Version7;

    /// <summary>
    /// Indicates the availability of user-verifying platform authenticator (e.g. Windows Hello).
    /// </summary>
    public static bool IsUserVerifyingPlatformAuthenticatorAvailable
    {
        get
        {
            try
            {
                HResult result = NativeMethods.IsUserVerifyingPlatformAuthenticatorAvailable(out bool value);
                ApiHelper.Validate(result);
                return value;
            }
            catch (TypeLoadException)
            {
                // If the IsUserVerifyingPlatformAuthenticatorAvailable function cannot be found, the feature is definitely not supported.
                return false;
            }
        }
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="WebAuthnApi"/> class.
    /// </summary>
    public WebAuthnApi()
    {
        if (!IsAvailable)
        {
            throw new NotSupportedException("The WebAuthN API is not supported on this OS.");
        }

        _cancellationId = GetCancellationId();
    }

    /// <summary>
    /// Produces an assertion signature representing an assertion by the authenticator that the user has consented to a specific transaction, such as logging in or completing a purchase.
    /// </summary>
    public AuthenticatorAssertionResponse AuthenticatorGetAssertion(
        string rpId,
        byte[] challenge,
        UserVerificationRequirement userVerificationRequirement,
        AuthenticatorAttachment authenticatorAttachment = AuthenticatorAttachment.Any,
        int timeoutMilliseconds = ApiConstants.DefaultTimeoutMilliseconds,
        IReadOnlyList<PublicKeyCredentialDescriptor> allowCredentials = null,
        AuthenticationExtensionsClientInputs extensions = null,
        CredentialLargeBlobOperation largeBlobOperation = CredentialLargeBlobOperation.None,
        byte[] largeBlob = null,
        bool browserInPrivateMode = false,
        HybridStorageLinkedData linkedDevice = null,
        WindowHandle windowHandle = default
    )
    {
        if (rpId == null)
        {
            throw new ArgumentNullException(nameof(rpId));
        }

        if (challenge == null)
        {
            throw new ArgumentNullException(nameof(challenge));
        }

        // TODO: Handle U2F attachment

        // Add "https://" to RpId if missing
        UriBuilder origin = new(rpId)
        {
            Scheme = Uri.UriSchemeHttps
        };

        CollectedClientData clientData = new()
        {
            Type = ApiConstants.ClientDataCredentialGet,
            Challenge = challenge,
            Origin = origin.Uri.ToString(),
            CrossOrigin = false
        };

        return AuthenticatorGetAssertion(
            rpId,
            clientData,
            userVerificationRequirement,
            authenticatorAttachment,
            timeoutMilliseconds,
            allowCredentials,
            extensions,
            largeBlobOperation,
            largeBlob,
            browserInPrivateMode,
            linkedDevice,
            windowHandle
        );
    }

    /// <summary>
    /// Produces an assertion signature representing an assertion by the authenticator that the user has consented to a specific transaction, such as logging in or completing a purchase.
    /// </summary>
    public AuthenticatorAssertionResponse AuthenticatorGetAssertion(
        string rpId,
        CollectedClientData clientData,
        UserVerificationRequirement userVerificationRequirement,
        AuthenticatorAttachment authenticatorAttachment = AuthenticatorAttachment.Any,
        int timeoutMilliseconds = ApiConstants.DefaultTimeoutMilliseconds,
        IReadOnlyList<PublicKeyCredentialDescriptor> allowCredentials = null,
        AuthenticationExtensionsClientInputs extensions = null,
        CredentialLargeBlobOperation largeBlobOperation = CredentialLargeBlobOperation.None,
        byte[] largeBlob = null,
        bool browserInPrivateMode = false,
        HybridStorageLinkedData linkedDevice = null,
        WindowHandle windowHandle = default
        )
    {
        if (rpId == null)
        {
            throw new ArgumentNullException(nameof(rpId));
        }

        if (clientData == null)
        {
            throw new ArgumentNullException(nameof(clientData));
        }

        if (extensions?.GetCredentialBlob == true && IsCredBlobSupported == false)
        {
            // This feature is only supported in API V3.
            throw new NotSupportedException("Credential blobs are not supported on this OS.");
        }

        if ((largeBlobOperation != CredentialLargeBlobOperation.None || largeBlob != null) && IsLargeBlobSupported == false)
        {
            // This feature is only supported in API V5.
            throw new NotSupportedException("Large blobs are not supported on this OS.");
        }

        if (browserInPrivateMode == true && IsPrivateBrowserModeIndicatorSupported == false)
        {
            // This feature is only supported in API V5.
            throw new NotSupportedException("The browser private mode indicator is not supported on this OS.");
        }

        if (extensions?.HmacGetSecret != null && IsPseudoRandomFunctionSupported == false)
        {
            // This feature is only supported in API V6.
            throw new NotSupportedException("The PRF extension is not supported on this OS.");
        }

        if (linkedDevice != null && IsHybridStorageLinkedDataSupported == false)
        {
            // This feature is only supported in API V7.
            throw new NotSupportedException("Hybrid storage linked data is not supported on this OS.");
        }

        if (!windowHandle.IsValid)
        {
            windowHandle = WindowHandle.ForegroundWindow;
        }

        using (DisposableList<CredentialIn> allowCreds = new())
        using (DisposableList<CredentialEx> allowCredsEx = new())
        {
            if (allowCredentials != null)
            {
                allowCreds.AddRange(allowCredentials.Select(credential =>
                    new CredentialIn(credential.Id, credential.Type)));
                allowCredsEx.AddRange(allowCredentials.Select(credential =>
                    new CredentialEx(credential.Id, credential.Type, credential.Transports)));
            }

            using (Credentials allowCredList = new(allowCreds.ToArray()))
            using (CredentialList allowCredListEx = new(allowCredsEx.ToArray()))
            using (ClientData clientDataNative = new(clientData))
            using (HmacSecretSaltIn globalHmacSalt = ApiHelper.Translate(extensions?.HmacGetSecret))
            using (HmacSecretSaltValuesIn hmacSecretSaltValues = new(globalHmacSalt, null))
            using (DisposableList<ExtensionIn> extensionsList = ApiHelper.Translate(extensions))
            using (ExtensionsIn nativeExtensions = new(extensionsList.ToArray()))
            using (AuthenticatorGetAssertionOptions options = new())
            {
                // Prepare native options
                options.TimeoutMilliseconds = timeoutMilliseconds;
                options.AuthenticatorAttachment = authenticatorAttachment;
                options.UserVerificationRequirement = userVerificationRequirement;
                options.AllowCredentials = allowCredList;
                options.AllowCredentialsEx = allowCredListEx;
                options.U2fAppId = extensions?.AppID;
                options.LargeBlobOperation = largeBlobOperation;
                options.Extensions = nativeExtensions;
                options.LargeBlob = largeBlob;
                options.BrowserInPrivateMode = browserInPrivateMode;
                options.HmacSecretSaltValues = hmacSecretSaltValues;
                options.LinkedDevice = linkedDevice;

                options.CancellationId = _cancellationId;

                // Perform the Win32 API call
                HResult result = NativeMethods.AuthenticatorGetAssertion(
                    windowHandle,
                    rpId,
                    clientDataNative,
                    options,
                    out AssertionSafeHandle assertionHandle
                );

                ApiHelper.Validate(result);

                try
                {
                    Assertion assertion = assertionHandle.ToManaged();

                    AuthenticationExtensionsClientOutputs extensionsOut = new()
                    {
                        HmacGetSecret = new HMACGetSecretOutput
                        {
                            Output1 = assertion.HmacSecret?.First,
                            Output2 = assertion.HmacSecret?.Second,
                        }
                    };

                    byte[] credBlob = assertion.Extensions?.CredBlob;

                    // Wrap the raw results
                    return new AuthenticatorAssertionResponse()
                    {
                        ClientDataJson = clientDataNative.ClientDataRaw,
                        AuthenticatorData = assertion.AuthenticatorData,
                        Signature = assertion.Signature,
                        UserHandle = assertion.UserId,
                        CredentialId = assertion.Credential.Id,
                    };
                }
                finally
                {
                    // Release native buffers.
                    assertionHandle.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Cancels the WebAuthn operation currently in progress.
    /// </summary>
    /// <remarks>
    /// When this operation is invoked by the client in an authenticator session,
    /// it has the effect of terminating any AuthenticatorMakeCredential or AuthenticatorGetAssertion operation
    /// currently in progress in that authenticator session.
    /// The authenticator stops prompting for, or accepting, any user input related to authorizing the canceled operation. The client ignores any further responses from the authenticator for the canceled operation.
    /// </remarks>
    public void CancelCurrentOperation()
    {
        if (_cancellationId.HasValue)
        {
            HResult result = NativeMethods.CancelCurrentOperation(_cancellationId.Value);
            ApiHelper.Validate(result);
        }
    }

    /// <summary>
    /// Gets the cancellation ID for a canceled operation.
    /// </summary>
    /// <returns>ID of the cancelled operation.</returns>
    private static Guid? GetCancellationId()
    {
        try
        {
            HResult result = NativeMethods.GetCancellationId(out Guid cancellationId);
            ApiHelper.Validate(result);
            return cancellationId;
        }
        catch (TypeLoadException)
        {
            // Async support is not present in earlier versions of Windows 10.
            return null;
        }
    }
}
