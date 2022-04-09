namespace AstroAdapters.Services
{
    public class StatusLogger : IStatusLogger
    {
        public event EventHandler<string>? OnStatusUpdated;
        public string LastStatus { get; private set; } = string.Empty;
        public void LogStatus(string status)
        {
            LastStatus = status.Trim();
            OnStatusUpdated?.Invoke(this, LastStatus);
        }

    }
}
