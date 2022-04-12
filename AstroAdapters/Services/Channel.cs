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
                foreach (var topicToCheck in new[] { string.Empty, topic })
                { 
                    if (subscribers[typeof(T)].ContainsKey(topicToCheck))
                    {
                        foreach (var subscriber in subscribers[typeof(T)][topicToCheck])
                        {
                            subscriber(topic, payload);
                        }
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
