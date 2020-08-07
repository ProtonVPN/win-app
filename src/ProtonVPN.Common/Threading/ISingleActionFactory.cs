using System;
using System.Threading.Tasks;

namespace ProtonVPN.Common.Threading
{
    public interface ISingleActionFactory
    {
        ISingleAction GetSingleAction(Func<Task> action);
    }
}
