using System;
using System.Threading.Tasks;

namespace ProtonVPN.Common.Threading
{
    public class SingleActionFactory : ISingleActionFactory
    {
        public ISingleAction GetSingleAction(Func<Task> action)
        {
            return new SingleAction(action);
        }
    }
}
