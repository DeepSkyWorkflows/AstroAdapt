using System.Text.Json;
using System.Text.Json.Serialization;
using AstroAdapt.Models;
using Microsoft.JSInterop;

namespace AstroAdapters.Services
{
    public sealed class DataServices : IDataServices
    {
        private readonly IJSRuntime jsRuntime;
        private const string ds = "dataServices";
        
        private readonly JsonSerializerOptions jsonOpts = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
        };

        public DataServices(IJSRuntime jsRuntime) =>
            this.jsRuntime = jsRuntime;
        
        public async Task<List<Manufacturer>?> GetManufacturersAsync() => await GetAsync<List<Manufacturer>>("getManufacturers");

        public async Task<Preferences?> GetPreferencesAsync() => await GetAsync<Preferences>("getPreferences");
        public async Task SavePreferencesAsync(Preferences preferences) => await SetAsync(preferences, "putPreferences");

        private async Task<T?> GetAsync<T>(string methodName)
        {
            var jsonStr = await jsRuntime.InvokeAsync<string>($"{ds}.{methodName}");
            if (!string.IsNullOrWhiteSpace(jsonStr))
            {
                var result = JsonSerializer.Deserialize<T>(jsonStr, jsonOpts);
                return result;
            }
            return default;
        }

        private async Task SetAsync<T>(T obj, string methodName)
        {
            var json = JsonSerializer.Serialize(obj, jsonOpts);
            var _ = await jsRuntime.InvokeAsync<bool>($"{ds}.{methodName}", json);
        }

        public async Task<string> ResolveImageAsync(Component component) =>
            await jsRuntime.InvokeAsync<string>($"{ds}.resolveImage", component.Id.ToString(), component.ComponentType.ToString());         

        public async Task<SavedSolution> SaveSolutionAsync(Solution solution, string description)
        {
            var solutions = await GetAsync<List<SavedSolution>>("getSolutions") ?? new List<SavedSolution>();
            var result = solution.ToSavedSolution();
            result.Description = description;
            solutions!.Add(result);
            await SetAsync(solutions, "putSolutions");
            return result;
        }
    }
}
