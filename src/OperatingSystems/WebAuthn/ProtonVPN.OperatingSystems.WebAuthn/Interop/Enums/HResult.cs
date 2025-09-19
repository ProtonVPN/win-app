namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Enums;

/// <summary>
/// Error codes relevant to the WebAuthN API.
/// </summary>
/// <remarks>Corresponds to HRESULT.</remarks>
public enum HResult : uint
{
    /// <summary>
    /// Operation successful
    /// </summary>
    /// <remarks>Corresponds to S_OK.</remarks>
    Success = 0,

    /// <summary>
    /// The action was cancelled by the user.
    /// </summary>
    /// <remarks>Corresponds to NTE_USER_CANCELLED.</remarks>
    ActionCancelled = 0x80090036,

    /// <summary>
    /// The operation was canceled by the user.
    /// </summary>
    /// <remarks>Corresponds to HRESULT_FROM_WIN32(ERROR_CANCELLED).</remarks>
    OperationCancelled = 0x800704C7,

    /// <summary>
    /// Object already exists.
    /// </summary>
    /// <remarks>Corresponds to NTE_EXISTS.</remarks>
    ObjectAlreadyExists = 0x8009000F,

    /// <summary>
    /// The data is invalid.
    /// </summary>
    /// <remarks>Corresponds to HRESULT_FROM_WIN32(ERROR_INVALID_DATA).</remarks>
    InvalidData = 0x8007000D,

    /// <summary>
    /// The request is not supported.
    /// </summary>
    /// <remarks>Corresponds to HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED).</remarks>
    RequestNotSupported = 0x80070032,

    /// <summary>
    /// The requested operation is not supported.
    /// </summary>
    /// <remarks>Corresponds to NTE_NOT_SUPPORTED.</remarks>
    OperationNotSupported = 0x80090029,

    /// <summary>
    /// The security token does not have storage space available for an additional container.
    /// </summary>
    /// <remarks>Corresponds to NTE_TOKEN_KEYSET_STORAGE_FULL.</remarks>
    KeyStorageFull = 0x80090023,

    /// <summary>
    /// The parameter is incorrect.
    /// </summary>
    /// <remarks>Corresponds to NTE_INVALID_PARAMETER.</remarks>
    ParameterInvalid = 0x80090027,

    /// <summary>
    /// The device that is required by this cryptographic provider is not found on this platform.
    /// </summary>
    /// <remarks>Corresponds to NTE_DEVICE_NOT_FOUND.</remarks>
    DeviceNotFound = 0x80090035,

    /// <summary>
    /// Object was not found.
    /// </summary>
    /// <remarks>Corresponds to NTE_NOT_FOUND.</remarks>
    ObjectNotFound = 0x80090011,

    /// <summary>
    /// This operation returned because the timeout period expired.
    /// </summary>
    /// <remarks>Corresponds to HRESULT_FROM_WIN32(ERROR_TIMEOUT).</remarks>
    OperationTimeout = 0x800705B4,

    /// <summary>
    /// The key container could not be opened.
    /// </summary>
    /// <remarks>Corresponds to NTE_BAD_KEYSET.</remarks>
    BadKeyset = 0x80090016,

    /// <summary>
    /// No smart card readers are available.
    /// </summary>
    /// <remarks>Corresponds to SCARD_E_NO_READERS_AVAILABLE.</remarks>
    NoReadersAvailable = 0x8010002E
}
