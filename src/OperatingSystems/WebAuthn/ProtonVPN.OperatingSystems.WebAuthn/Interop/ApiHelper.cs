using System.ComponentModel;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop;

public static class ApiHelper
{
    public static void Validate(HResult result)
    {
        if (result == HResult.Success)
        {
            // No error, so continue with code execution.
            return;
        }

        Win32Exception win32Exception = new(unchecked((int)result));

        // Try to wrap the generic Win32Exception with a more specific .NET exception type.
        switch (result)
        {
            case HResult.ActionCancelled:
            case HResult.OperationCancelled:
                throw new OperationCanceledException(win32Exception.Message, win32Exception);
            case HResult.OperationTimeout:
                throw new TimeoutException(win32Exception.Message, win32Exception);
            case HResult.RequestNotSupported:
            case HResult.OperationNotSupported:
                throw new NotSupportedException(win32Exception.Message, win32Exception);
            case HResult.ParameterInvalid:
            case HResult.InvalidData:
                throw new ArgumentException(win32Exception.Message, win32Exception);
            case HResult.ObjectAlreadyExists:
            case HResult.KeyStorageFull:
            case HResult.DeviceNotFound:
            case HResult.ObjectNotFound:
            default:
                // TODO: Differentiate between more error states using custom exception types.
                throw win32Exception;
        }
    }

    public static DisposableList<ExtensionIn> Translate(AuthenticationExtensionsClientInputs extensions)
    {
        DisposableList<ExtensionIn> nativeExtensions = new();

        if (extensions != null)
        {
            if (extensions.CredProtect != UserVerification.Any)
            {
                nativeExtensions.Add(ExtensionIn.CreateCredProtect(
                    extensions.CredProtect,
                    extensions.EnforceCredProtect == true));
            }

            if (extensions.HmacCreateSecret == true)
            {
                nativeExtensions.Add(ExtensionIn.CreateHmacSecret());
            }

            if (extensions.MinimumPinLength == true)
            {
                nativeExtensions.Add(ExtensionIn.CreateMinPinLength());
            }

            if (extensions.CredentialBlob != null)
            {
                nativeExtensions.Add(ExtensionIn.CreateCredBlobAttestation(extensions.CredentialBlob));
            }

            if (extensions.GetCredentialBlob == true)
            {
                nativeExtensions.Add(ExtensionIn.CreateCredBlobAssertion());
            }
        }

        return nativeExtensions;
    }

    public static HmacSecretSaltIn Translate(HMACGetSecretInput salts)
    {
        if (salts == null)
        {
            return null;
        }

        return new HmacSecretSaltIn(salts.Salt1, salts.Salt2);
    }
}
