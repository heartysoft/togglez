using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Togglez.Internal
{
    public class Togglez : global::Togglez.Togglez
    {
        private readonly JObject _settings = new JObject();
        readonly Dictionary<string, List<ToggleSubscription>> _subscriptions = new Dictionary<string, List<ToggleSubscription>>(); 
        private readonly TaskCompletionSource<bool> _initialSet = new TaskCompletionSource<bool>(); 

        readonly object _locker = new object();
        public void Set(string json)
        {
            lock (_locker)
            {
                _initialSet.TrySetResult(true);

                var notifyList = new List<string>();
                
                var newSettings = JObject.Parse(json);

                foreach (var newSetting in newSettings)
                {
                    var oldValue = _settings[newSetting.Key];

                    if (oldValue == null && newSetting.Value == null)
                        continue;

                    if(oldValue != null && oldValue.Value<string>() == newSetting.Value.Value<string>())
                        continue;

                    
                    notifyList.Add(newSetting.Key);
                }
                
                _settings.Merge(newSettings);

                foreach (var property in newSettings)
                {
                    if(!notifyList.Contains(property.Key))
                        continue;

                    var list = getList(property.Key);
                    foreach (var action in list)
                        action.Handle(property.Value);
                }
            }
        }

        public bool IsOn(string toggle)
        {
            return Get<bool>(toggle);
        }

        public T Get<T>(string toggle)
        {
            var val = _settings[toggle];
            
            if (val != null)
                return val.Value<T>();

            return default(T);
        }

        public Func<T> GetFactory<T>(string toggle)
        {
            return () => Get<T>(toggle);
        }

        public void SubscribeOn<T>(string toggle, Action<T> handler)
        {
            var list = getList(toggle);

            list.Add(new ToggleSubscription(typeof(T), x => handler((T)x)));
        }
        public void WaitForFirstSettings(TimeSpan timeout)
        {
            waitForSettings(timeout,
                _initialSet.Task.Wait((int)timeout.TotalMilliseconds));
        }

        public void WaitForFirstSettings(TimeSpan timeout, CancellationToken cancel)
        {
            waitForSettings(timeout, 
                _initialSet.Task.Wait((int)timeout.TotalMilliseconds, cancel));
        }

        private void waitForSettings(TimeSpan timeout, bool waitResult)
        {
            if (!waitResult)
            {
                throw new SettingsNotReceivedWithinTimeoutException(timeout);
            }
        }

        private List<ToggleSubscription> getList(string toggle)
        {
            List<ToggleSubscription> list;

            if (_subscriptions.ContainsKey(toggle) == false)
            {
                list = new List<ToggleSubscription>();
                _subscriptions[toggle] = list;
            }
            else list = _subscriptions[toggle];

            return list;
        }

        public class SettingsNotReceivedWithinTimeoutException : Exception
        {
            public SettingsNotReceivedWithinTimeoutException(TimeSpan timeout)
                : base(string.Format("[Togglez] Did not receive initial settings within specified timeout {0}.", timeout))
            {
            }
        }
    }
}