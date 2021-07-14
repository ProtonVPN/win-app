using System.Collections.Generic;

namespace ProtonVPN.Streaming
{
    internal interface IStreamingServices
    {
        IReadOnlyList<StreamingService> GetServices(string countryCode, sbyte tier);
    }
}