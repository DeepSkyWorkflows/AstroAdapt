namespace AstroAdapters.Services
{
    public interface IStatusLogger
    {
        string LastStatus { get; }

        event EventHandler<string>? OnStatusUpdated;

        void LogStatus(string status);
    }
}