using System.Diagnostics;
using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop;

namespace ProtonVPN.OperatingSystems.WebAuthn;

/// <summary>
/// Represents a window handle.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct WindowHandle
{
    private nint _handle;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    public WindowHandle(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Indicates whether the window handle is valid.
    /// </summary>
    public bool IsValid => _handle != nint.Zero;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator ==(WindowHandle a, WindowHandle b) => a._handle == b._handle;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator !=(WindowHandle a, WindowHandle b) => a._handle != b._handle;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) => obj is WindowHandle handle && handle._handle == _handle;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => _handle.GetHashCode();

    /// <summary>
    /// Gets the window handle of the foreground window associated with the calling process.
    /// </summary>
    public static WindowHandle ForegroundWindow => NativeMethods.GetForegroundWindow();

    /// <summary>
    /// Gets the window handle of the main window of the calling process.
    /// </summary>
    public static WindowHandle MainWindow => new(Process.GetCurrentProcess().MainWindowHandle);

    /// <summary>
    /// Retrieves the window handle used by the console associated with the calling process.
    /// </summary>
    public static WindowHandle ConsoleWindow => NativeMethods.GetConsoleWindow();
}
