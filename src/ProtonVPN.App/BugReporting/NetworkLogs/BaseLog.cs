namespace ProtonVPN.BugReporting.NetworkLogs
{
    public abstract class BaseLog : ILog
    {
        private readonly string _path;
        private readonly string _filename;

        protected BaseLog(string path, string filename)
        {
            _path = path;
            _filename = filename;
        }

        public abstract void Write();

        public string Path => System.IO.Path.Combine(_path, _filename);
    }
}
