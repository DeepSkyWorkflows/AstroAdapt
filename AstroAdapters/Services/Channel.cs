namespace AstroAdapters.Services
{
    public class Channel : IChannel
    {
        private readonly IDictionary<Type, IDictionary<string, IList<Action<string, object?>>>> subscribers =
            new Dictionary<Type, IDictionary<string, IList<Action<string, object?>>>>();

        public void Publish<T>(string topic, T? payload)
            where T : class
        {
            if (subscribers.ContainsKey(typeof(T)))
            {
                if (subscribers[typeof(T)].ContainsKey(topic))
                {
                    foreach (var subscriber in subscribers[typeof(T)][topic])
                    {
                        subscriber(topic, payload);
                    }
                }
            }
        }

        public void Subscribe<T>(string topic, Action<string, T?> action)
            where T : class
        {
            if (!subscribers.ContainsKey(typeof(T)))
            {
                subscribers.Add(typeof(T), new Dictionary<string, IList<Action<string, object?>>>());                    
            }

            if (!subscribers[typeof(T)].ContainsKey(topic))
            {
                subscribers[typeof(T)][topic] = new List<Action<string, object?>>();
            }

            subscribers[typeof(T)][topic].Add((o, e) => action(o, e as T));
        }
    }
}
