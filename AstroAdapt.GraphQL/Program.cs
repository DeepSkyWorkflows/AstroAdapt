using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.GraphQL;
using AstroAdapt.Models;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var path = new AstroApp().PathToDatabase;

builder.Services.AddSingleton<IAstroApp>(sp => new AstroApp());

builder.Services.AddPooledDbContextFactory<AstroContext>(
    opts => opts.UseSqlite($"Data Source={path}"));

builder.Services.AddSingleton<SolutionProcessingService>();

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddAstroTypes()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .RegisterService<IAstroApp>()
    .RegisterService<ITopicEventReceiver>()
    .RegisterService<ITopicEventSender>()
    .RegisterDbContext<AstroContext>(DbContextKind.Pooled)
    .AddInMemorySubscriptions();

builder.Services.AddCors(
    p => p.AddPolicy("demo",
        p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));    

var app = builder.Build();

app.UseCors("demo");
app.UseRouting();
app.UseWebSockets();
app.UseEndpoints(endpoints => endpoints.MapGraphQL());

var provider = app.Services.CreateScope().ServiceProvider;
var astroApp = provider.GetService<IAstroApp>();
await astroApp!.InitializeAsync(
    path =>
    provider.GetRequiredService<IDbContextFactory<AstroContext>>());

app.Run();
