using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public interface IDataServices
    {
        Task<Preferences?> GetPreferencesAsync();
        Task SavePreferencesAsync(Preferences preferences);
        Task<SavedSolution> SaveSolutionAsync(Solution solution, string description);
        Task<List<Manufacturer>?> GetManufacturersAsync();
        Task<string> ResolveImageAsync(Component component);
    }
}
