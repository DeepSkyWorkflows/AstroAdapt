using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public interface IAppHost
    {
        event EventHandler<string> OnStatusUpdated;
        Task<List<Component>> GetComponentsAsync();
        Task<Preferences> GetPreferencesAsync();
        Task SavePreferencesAsync(Preferences preferences);
        Task<List<Manufacturer>> GetManufacturersAsync();
        void LogStatus(string status);
        string LastStatus { get; }
        bool IsBusy { get; }
        void SetBusy();
        void ResetBusy();
    }
}
