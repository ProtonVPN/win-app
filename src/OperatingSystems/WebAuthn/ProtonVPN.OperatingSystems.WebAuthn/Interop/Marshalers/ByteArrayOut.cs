using System.Runtime.InteropServices;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

[StructLayout(LayoutKind.Sequential)]
public class ByteArrayOut
{
    protected nint _nativeArray = nint.Zero;

    // This class is only created by marshaling.
    protected ByteArrayOut() { }

    public byte[] Read(int length)
    {
        if (_nativeArray == nint.Zero || length <= 0)
        {
            return null;
        }

        byte[] managedArray = new byte[length];
        Marshal.Copy(_nativeArray, managedArray, 0, length);

        return managedArray;
    }
}