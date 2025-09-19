using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about Extension.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_EXTENSION.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class ExtensionIn : IDisposable
{
    /// <summary>
    /// Extension identifier.
    /// </summary>
    public string Identifier { get; private set; }

    private int _dataLength;

    /// <summary>
    /// Extension data.
    /// </summary>
    private ByteArrayIn _data;

    public ExtensionIn(string id, byte[] data)
    {
        Identifier = id;
        _dataLength = data?.Length ?? 0;
        _data = new ByteArrayIn(data);
    }

    public static ExtensionIn CreateHmacSecret()
    {
        // Below type definitions is for WEBAUTHN_EXTENSIONS_IDENTIFIER_HMAC_SECRET
        // MakeCredential Input Type:   BOOL.
        //      - pvExtension must point to a BOOL with the value TRUE.
        //      - cbExtension must contain the sizeof(BOOL).
        byte[] binaryTrue = BitConverter.GetBytes(1);
        return new ExtensionIn(ApiConstants.ExtensionIdentifierHmacSecret, binaryTrue);
    }

    public static ExtensionIn CreateCredProtect(UserVerification uv, bool enforce)
    {
        // Below type definitions is for WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_PROTECT
        // MakeCredential Input Type:   WEBAUTHN_CRED_PROTECT_EXTENSION_IN.
        //      - pvExtension must point to a WEBAUTHN_CRED_PROTECT_EXTENSION_IN struct
        //      - cbExtension will contain the sizeof(WEBAUTHN_CRED_PROTECT_EXTENSION_IN).
        int structSize = sizeof(UserVerification) + sizeof(int);
        byte[] extensionData = new byte[structSize];
        BitConverter.GetBytes((int)uv).CopyTo(extensionData, 0);
        BitConverter.GetBytes(enforce ? 1 : 0).CopyTo(extensionData, sizeof(UserVerification));
        return new ExtensionIn(ApiConstants.ExtensionIdentifierCredProtect, extensionData);
    }

    public static ExtensionIn CreateCredBlobAttestation(byte[] blob)
    {
        if (blob == null)
        {
            throw new ArgumentNullException(nameof(blob));
        }

        // Below type definitions is for WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_BLOB
        // MakeCredential Input Type:   WEBAUTHN_CRED_BLOB_EXTENSION.
        //      - pvExtension must point to a WEBAUTHN_CRED_BLOB_EXTENSION struct
        //      - cbExtension must contain the sizeof(WEBAUTHN_CRED_BLOB_EXTENSION).
        int structSize = blob.Length + sizeof(int);
        byte[] extensionData = new byte[structSize];
        BitConverter.GetBytes(blob.Length).CopyTo(extensionData, 0);
        blob.CopyTo(extensionData, sizeof(int));
        return new ExtensionIn(ApiConstants.ExtensionIdentifierCredBlob, extensionData);
    }

    public static ExtensionIn CreateCredBlobAssertion()
    {
        // Below type definitions is for WEBAUTHN_EXTENSIONS_IDENTIFIER_CRED_BLOB
        // GetAssertion Input Type:     BOOL.
        //      - pvExtension must point to a BOOL with the value TRUE to request the credBlob.
        //      - cbExtension must contain the sizeof(BOOL).
        byte[] binaryTrue = BitConverter.GetBytes(1);
        return new ExtensionIn(ApiConstants.ExtensionIdentifierCredBlob, binaryTrue);
    }

    public static ExtensionIn CreateMinPinLength()
    {
        // Below type definitions is for WEBAUTHN_EXTENSIONS_IDENTIFIER_MIN_PIN_LENGTH
        // MakeCredential Input Type:   BOOL.
        //      - pvExtension must point to a BOOL with the value TRUE to request the minPinLength.
        //      - cbExtension must contain the sizeof(BOOL).
        byte[] binaryTrue = BitConverter.GetBytes(1);
        return new ExtensionIn(ApiConstants.ExtensionIdentifierMinPinLength, binaryTrue);
    }

    public void Dispose()
    {
        _data?.Dispose();
        _data = null;
    }
}