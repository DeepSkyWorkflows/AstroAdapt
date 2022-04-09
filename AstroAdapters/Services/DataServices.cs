using System.Text.Json;
using System.Text.Json.Serialization;
using AstroAdapt.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AstroAdapters.Services
{
    public sealed class DataServices : IDataServices
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private readonly JsonSerializerOptions jsonOpts = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
        };

        public DataServices(IJSRuntime jsRuntime) =>
           moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import",
               "./js/dataServices.js")
           .AsTask()!);

        public async Task<List<Manufacturer>?> GetManufacturersAsync() => await GetAsync<List<Manufacturer>>("getManufacturers");

        public async Task<Preferences?> GetPreferencesAsync() => await GetAsync<Preferences>("getPreferences");
        public async Task SavePreferencesAsync(Preferences preferences) => await SetAsync(preferences, "putPreferences");
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

        private async Task<T?> GetAsync<T>(string methodName)
        {
            var module = await moduleTask.Value;
            var jsonStr = await module.InvokeAsync<string>(methodName);
            if (!string.IsNullOrWhiteSpace(jsonStr))
            {
                var result = JsonSerializer.Deserialize<T>(jsonStr, jsonOpts);
                return result;
            }
            return default;
        }

        private async Task SetAsync<T>(T obj, string methodName)
        {
            var module = await moduleTask.Value;
            var json = JsonSerializer.Serialize(obj, jsonOpts);
            var _ = await module.InvokeAsync<bool>(methodName, json);
        }

        public async Task<string> ResolveImageAsync(Component component)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("resolveImage", component.Id.ToString(), component.ComponentType.ToString()); 
        }
    }
}
