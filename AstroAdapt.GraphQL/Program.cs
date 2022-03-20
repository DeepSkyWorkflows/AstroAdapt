using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.GraphQL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var path = new AstroApp().PathToDatabase;

builder.Services.AddScoped<IAstroApp>(sp => new AstroApp());

builder.Services.AddPooledDbContextFactory<AstroContext>(
    opts => opts.UseSqlite($"Data Source={path}"));

builder.Services.AddGraphQLServer()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddQueryType<Query>()
   //  .AddMutationType<Mutations>()
   //.AddTypeExtension<ObservationTypeExtensions>()
   //  .AddSubscriptionType<SubscriptionsType>()
   //.AddInMemorySubscriptions()
   ;

var app = builder.Build();

app.MapGraphQL();
app.UseRouting();

var provider = app.Services.CreateScope().ServiceProvider;
var astroApp = provider.GetService<IAstroApp>();
await astroApp!.InitializeAsync(
    path =>
    provider.GetRequiredService<IDbContextFactory<AstroContext>>());

app.Run();
