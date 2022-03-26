using AstroAdapt.BlazorWasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAstroClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7247/graphql"))
    .ConfigureWebSocketClient(client => client.Uri = new Uri("wss://localhost:7247/graphql"));    

await builder.Build().RunAsync();
