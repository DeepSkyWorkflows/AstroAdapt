namespace AstroAdapters.Services
{
    public interface IChannel
    {
        void Subscribe<T>(string topic, Action<string, T?> action)
            where T: class;
        void Publish<T>(string topic, T? payload)
            where T: class;
    }
}
