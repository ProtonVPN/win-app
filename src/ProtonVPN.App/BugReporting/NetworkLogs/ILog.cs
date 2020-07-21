namespace ProtonVPN.BugReporting.NetworkLogs
{
    public interface ILog
    {
        string Path { get; }

        void Write();
    }
}
