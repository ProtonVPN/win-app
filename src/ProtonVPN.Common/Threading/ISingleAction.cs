using System;
using System.Threading.Tasks;

namespace ProtonVPN.Common.Threading
{
    public interface ISingleAction
    {
        event EventHandler<TaskCompletedEventArgs> Completed;
        Task Task { get; }
        bool IsRunning { get; }
        Task Run();
        void Cancel();
    }
}