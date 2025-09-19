namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

public class DisposableList<T> : List<T>, IDisposable where T : IDisposable
{
    public void Dispose()
    {
        foreach (T item in this)
        {
            item?.Dispose();
        }
    }
}
