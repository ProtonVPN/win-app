using System.Runtime.InteropServices;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

[StructLayout(LayoutKind.Sequential)]
public sealed class ByteArrayIn : ByteArrayOut, IDisposable
{
    public ByteArrayIn(byte[] data)
    {
        if ((data?.Length ?? 0) > 0)
        {
            _nativeArray = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, _nativeArray, data.Length);
        }
    }

    public void Dispose()
    {
        if (_nativeArray != nint.Zero)
        {
            Marshal.FreeHGlobal(_nativeArray);
            _nativeArray = nint.Zero;
        }
    }
}
