using ProtonVPN.Core.Helpers;

namespace ProtonVPN.Core.Abstract
{
    public class File
    {
        public string Name { get; }
        public byte[] Content { get; }

        public File(string name, byte[] content)
        {
            Ensure.NotEmpty(name, nameof(name));
            Ensure.NotEmpty(content, nameof(content));

            Name = name;
            Content = content;
        }
    }
}
