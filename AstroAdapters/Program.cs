using AstroAdapters;
using AstroAdapters.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IStatusLogger, StatusLogger>();
builder.Services.AddSingleton<IDataServices, DataServices>();
builder.Services.AddSingleton<IAppHost, AppHost>();
builder.Services.AddSingleton<ISolverWizard, SolverWizard>();
builder.Services.AddSingleton<IChannel, Channel>();

await builder.Build().RunAsync();
