using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public sealed class AppHost : IAppHost
    {
        private readonly List<Manufacturer> manufacturers = new();
        private readonly IDataServices dataServices;
        private readonly IStatusLogger statusLogger;
        private readonly IChannel channel;
        private int busyCount = 0;
        private Preferences? prefs = null;

        public AppHost(IDataServices dataServices, IStatusLogger statusLogger, IChannel channel)
        {
            this.dataServices = dataServices;
            this.statusLogger = statusLogger;
            this.channel = channel;
            statusLogger.LogStatus("App host initialized.");
        }

        public bool IsBusy
        {
            get => busyCount > 0;
        }

        
        public void SetBusy()
        {
            busyCount++;
            if (busyCount == 1)
            {
                channel.Publish(nameof(SetBusy), new Box<bool>(true));
            }
        }
        public void ResetBusy()
        {
            busyCount--;
            if (busyCount == 0)
            {
                channel.Publish(nameof(SetBusy), new Box<bool>(false)); ;
            }
        }

        public async Task<List<Manufacturer>> GetManufacturersAsync()
        {
            statusLogger.LogStatus($"Retrieving manufacturers and components...");
            SetBusy();
            var loadedManufacturers = await dataServices.GetManufacturersAsync();
            ResetBusy();
            if (loadedManufacturers != null)
            {
                statusLogger.LogStatus($"{loadedManufacturers.Count} manufacturers with {loadedManufacturers.SelectMany(m => m.Components).Count()} components loaded.");
                manufacturers.Clear();
                manufacturers.AddRange(loadedManufacturers);
            }
            else
            {
                statusLogger.LogStatus("No manufacturers found.");
            }
            return manufacturers;
        }
                
        public async Task<List<Component>> GetComponentsAsync() =>
        (manufacturers.Any() ? manufacturers.ToList() : await GetManufacturersAsync())
            .SelectMany(m => m.Components).ToList();

        public async Task<Preferences> GetPreferencesAsync()
        {
            if (prefs == null)
            {
                statusLogger.LogStatus($"Retrieving preferences...");
                SetBusy();
                prefs = await dataServices.GetPreferencesAsync();
                ResetBusy();
                if (prefs != null)
                {
                    statusLogger.LogStatus($"Preferences loaded.");
                }
                else
                {
                    prefs = new Preferences();
                    statusLogger.LogStatus($"No preferences found. Using defaults.");
                }
            }
            return prefs;
        }

        public async Task SavePreferencesAsync(Preferences preferences)
        {
            statusLogger.LogStatus($"Saving preferences...");
            SetBusy();
            await dataServices.SavePreferencesAsync(preferences);
            ResetBusy();
            statusLogger.LogStatus($"Preferences saved.");
            prefs = preferences;
        }
    }
}
