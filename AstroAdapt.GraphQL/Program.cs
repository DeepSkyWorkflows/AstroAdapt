using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.GraphQL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var path = new AstroApp().PathToDatabase;

builder.Services.AddSingleton<IAstroApp>(sp => new AstroApp());

builder.Services.AddPooledDbContextFactory<AstroContext>(
    opts => opts.UseSqlite($"Data Source={path}"));

builder.Services.AddSingleton<SolutionProcessingService>();

builder.Services.AddGraphQLServer()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddQueryType<Query>()
   //  .AddMutationType<Mutations>()
   //.AddTypeExtension<ObservationTypeExtensions>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions()
   ;

var app = builder.Build();

app.UseRouting();
app.UseWebSockets();
app.UseEndpoints(endpoints => endpoints.MapGraphQL());

var provider = app.Services.CreateScope().ServiceProvider;
var astroApp = provider.GetService<IAstroApp>();
await astroApp!.InitializeAsync(
    path =>
    provider.GetRequiredService<IDbContextFactory<AstroContext>>());

app.Run();
