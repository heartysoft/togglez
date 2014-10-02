using System;
using Newtonsoft.Json.Linq;

namespace Togglez.Internal
{
    public class ToggleSubscription
    {
        private readonly Type _type;
        private readonly Action<object> _handler;

        public ToggleSubscription(Type type, Action<object> handler)
        {
            _type = type;
            _handler = handler;
        }

        public void Handle(JToken value)
        {
            var val = value.ToObject(_type);
            _handler(val);
        }
    }
}