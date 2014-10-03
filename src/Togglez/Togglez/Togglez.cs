using System;
using System.Threading;

namespace Togglez
{
    public interface Togglez
    {
        bool IsOn(string toggle);
        T Get<T>(string toggle);
        Func<T> GetFactory<T>(string toggle);
        void SubscribeOn<T>(string toggle, Action<T> handler);
        void WaitForFirstSettings(TimeSpan timeout);
        void WaitForFirstSettings(TimeSpan timeout, CancellationToken cancel);
    }
}