using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public interface IDataServices : IAsyncDisposable
    {
        Task<Preferences?> GetPreferencesAsync();
        Task SavePreferencesAsync(Preferences preferences);
        Task<List<Manufacturer>?> GetManufacturersAsync();        
    }
}
