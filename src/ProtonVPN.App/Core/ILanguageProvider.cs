using System.Collections.Generic;

namespace ProtonVPN.Core
{
    public interface ILanguageProvider
    {
        List<string> GetAll();
    }
}
