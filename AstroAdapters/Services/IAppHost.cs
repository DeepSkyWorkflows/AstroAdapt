using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public interface IAppHost
    {
        Task<List<Component>> GetComponentsAsync();
        Task<Preferences> GetPreferencesAsync();
        Task SavePreferencesAsync(Preferences preferences);
        Task<List<Manufacturer>> GetManufacturersAsync();
        bool IsBusy { get; }
        void SetBusy();
        void ResetBusy();
    }
}
