using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public sealed class AppHost : IAsyncDisposable, IAppHost
    {
        private readonly List<Manufacturer> manufacturers = new();
        private readonly List<SavedSolution> savedSolutions = new();
        private readonly IDataServices dataServices;
        private int busyCount = 0;
        private Preferences? prefs = null;

        public AppHost(IDataServices dataServices)
            => this.dataServices = dataServices;
            
        public event EventHandler<string>? OnStatusUpdated;

        public bool IsBusy
        {
            get => busyCount > 0;
        }

        public string LastStatus { get; private set; } = string.Empty;

        public async ValueTask DisposeAsync() => await dataServices.DisposeAsync();
        
        public void SetBusy() => busyCount++;
        public void ResetBusy() => busyCount--;

        public async Task<List<Manufacturer>> GetManufacturersAsync()
        {
            LogStatus($"Retrieving manufacturers and components...");
            SetBusy();
            var loadedManufacturers = await dataServices.GetManufacturersAsync();
            ResetBusy();
            if (loadedManufacturers != null)
            {
                LogStatus($"{loadedManufacturers.Count} manufacturers with {loadedManufacturers.SelectMany(m => m.Components).Count()} components loaded.");
                manufacturers.Clear();
                manufacturers.AddRange(loadedManufacturers);
            }
            else
            {
                LogStatus("No manufacturers found.");
            }
            return manufacturers;
        }

        public void LogStatus(string status)
        {
            LastStatus = status.Trim();
            OnStatusUpdated?.Invoke(this, LastStatus);
        }

        public async Task<List<Component>> GetComponentsAsync() =>
        (manufacturers.Any() ? manufacturers.ToList() : await GetManufacturersAsync())
            .SelectMany(m => m.Components).ToList();

        public async Task<Preferences> GetPreferencesAsync()
        {
            if (prefs == null)
            {
                LogStatus($"Retrieving preferences...");
                SetBusy();
                prefs = await dataServices.GetPreferencesAsync();
                ResetBusy();
                if (prefs != null)
                {
                    LogStatus($"Preferences loaded.");
                }
                else
                {
                    prefs = new Preferences();
                    LogStatus($"No preferences found. Using defaults.");
                }
            }
            return prefs;
        }

        public async Task SavePreferencesAsync(Preferences preferences)
        {
            LogStatus($"Saving preferences...");
            SetBusy();
            await dataServices.SavePreferencesAsync(preferences);
            ResetBusy();
            LogStatus($"Preferences saved.");
            prefs = preferences;
        }
    }
}
