using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs.GetAssertion;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop;

/// <summary>
/// This class exposes native WebAuthn API implemented in webauthn.dll in Windows 10.
/// </summary>
/// <see>https://github.com/microsoft/webauthn/blob/master/webauthn.h</see>
public static class NativeMethods
{
    private const string WebAuthn = "webauthn.dll";
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";

    /// <summary>
    /// Gets the version number of the WebAuthN API.
    /// </summary>
    /// <returns>The WebAuthN API version number.</returns>
    [DllImport(WebAuthn, EntryPoint = "WebAuthNGetApiVersionNumber")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern ApiVersion GetApiVersionNumber();

    /// <summary>
    /// Determines whether the platform authenticator service is available.
    /// </summary>
    /// <param name="isUserVerifyingPlatformAuthenticatorAvailable">True if and only if a user-verifying platform authenticator is available.</param>
    /// <returns>If the function succeeds, it returns S_OK. If the function fails, it returns an HRESULT value that indicates the error.</returns>
    [DllImport(WebAuthn, EntryPoint = "WebAuthNIsUserVerifyingPlatformAuthenticatorAvailable")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern HResult IsUserVerifyingPlatformAuthenticatorAvailable(out bool isUserVerifyingPlatformAuthenticatorAvailable);

    /// <summary>
    /// Produces an assertion signature representing an assertion by the authenticator
    /// that the user has consented to a specific transaction, such as logging in or completing a purchase.
    /// </summary>
    /// <param name="windowHandle">The handle for the window that will be used to display the UI.</param>
    /// <param name="rpId">The ID of the Relying Party.</param>
    /// <param name="clientData">The client data to be sent to the authenticator for the Relying Party.</param>
    /// <param name="getAssertionOptions">The options for the WebAuthNAuthenticatorGetAssertion operation.</param>
    /// <param name="assertion">A pointer to a WEBAUTHN_ASSERTION that receives the assertion.</param>
    /// <returns>If the function succeeds, it returns S_OK. If the function fails, it returns an HRESULT value that indicates the error.</returns>
    /// <remarks>
    /// If the authenticator cannot find any credential corresponding to the specified Relying Party that matches the specified criteria, it terminates the operation and returns an error.
    /// Before performing this operation, all other operations in progress in the authenticator session MUST be aborted by running the WebAuthNCancelCurrentOperation operation.
    /// </remarks>
    [DllImport(WebAuthn, EntryPoint = "WebAuthNAuthenticatorGetAssertion", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern HResult AuthenticatorGetAssertion(
        WindowHandle windowHandle,
        string rpId,
        ClientData clientData,
        AuthenticatorGetAssertionOptions getAssertionOptions,
        out AssertionSafeHandle assertion
    );

    /// <summary>
    /// Frees memory that the AuthenticatorGetAssertion function has allocated for a WEBAUTHN_ASSERTION structure.
    /// </summary>
    /// <param name="webAuthNAssertion">Pointer to a WEBAUTHN_ASSERTION structure to deallocate.</param>
    [DllImport(WebAuthn, EntryPoint = "WebAuthNFreeAssertion")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern void FreeAssertion(IntPtr webAuthNAssertion);

    /// <summary>
    /// Gets the cancellation ID for the canceled operation.
    /// </summary>
    /// <param name="cancellationId">The GUID returned, representing the ID of the cancelled operation.</param>
    /// <returns>If the function succeeds, it returns S_OK. If the function fails, it returns an HRESULT value that indicates the error.</returns>
    [DllImport(WebAuthn, EntryPoint = "WebAuthNGetCancellationId")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern HResult GetCancellationId(out Guid cancellationId);

    /// <summary>
    /// When this operation is invoked by the client in an authenticator session, it has the effect
    /// of terminating any WebAuthNAuthenticatorMakeCredential or WebAuthNAuthenticatorGetAssertion
    /// operation currently in progress in that authenticator session. The authenticator stops prompting for,
    /// or accepting, any user input related to authorizing the canceled operation.
    /// The client ignores any further responses from the authenticator for the canceled operation.
    /// </summary>
    /// <param name="cancellationId">The GUID returned, representing the ID of the cancelled operation.</param>
    /// <returns>If the function succeeds, it returns S_OK. If the function fails, it returns an HRESULT value that indicates the error.</returns>
    /// <remarks>
    /// This operation is ignored if it is invoked in an authenticator session which does not have
    /// an WebAuthNAuthenticatorMakeCredential or WebAuthNAuthenticatorGetAssertion operation currently in progress.
    /// </remarks>
    [DllImport(WebAuthn, EntryPoint = "WebAuthNCancelCurrentOperation")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern HResult CancelCurrentOperation(in Guid cancellationId);

    /// <summary>
    /// Retrieves a handle to the foreground window (the window with which the user is currently working).
    /// </summary>
    /// <returns>The return value is a handle to the foreground window. The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</returns>
    [DllImport(User32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern WindowHandle GetForegroundWindow();

    /// <summary>
    /// Retrieves the window handle used by the console associated with the calling process.
    /// </summary>
    /// <returns>The return value is a handle to the window used by the console associated with the calling process or NULL if there is no such associated console.</returns>
    [DllImport(Kernel32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern WindowHandle GetConsoleWindow();
}
