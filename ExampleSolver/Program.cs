// See https://aka.ms/new-console-template for more information

using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var refreshRateMs = TimeSpan.FromMilliseconds(1000).Ticks;
var lastTick = DateTime.Now.Ticks;
object statusContext = new();

Console.WriteLine("Example solver.");

Console.WriteLine("Initializing engine...");

var engine = new AstroApp();
await engine.InitializeAsync(path =>
{
    var sc = new ServiceCollection();
    sc.AddDbContextFactory<AstroContext>(opts => opts.UseSqlite($"Data Source={path}"));
    var sp = sc.BuildServiceProvider();
    return sp.GetRequiredService<IDbContextFactory<AstroContext>>();
});

var componentList = await engine.LoadInventoryAsync();
var targets = componentList.Where(c => c.TargetDirectionConnectionType == ConnectionTypes.Terminator);
var sensors = componentList.Where(c => c.SensorDirectionConnectionType == ConnectionTypes.Terminator);
var connectors = componentList.Where(c => c.SensorDirectionConnectionType != ConnectionTypes.Terminator
 && c.TargetDirectionConnectionType != ConnectionTypes.Terminator);

foreach (var target in targets)
{
    foreach (var sensor in sensors)
    {
        Console.WriteLine($"Solving image train from {target} to {sensor}.");
        Console.WriteLine($"s=skip,b=barlow,c=compresion ring,f=filter wheel");
        Console.Write("Enter commands: ");
        var response = Console.ReadLine() ?? string.Empty;
        response = response.Trim().ToLowerInvariant();
        if (response == "s")
        {
            continue;
        }
        var compression = response.Contains('c');
        var barlow = response.Contains('b');
        var filterwheel = response.Contains('f');
        Console.Write($"Backfocus tolerance (ENTER=0.001 or 0.1%): ");
        var bf = Console.ReadLine() ?? string.Empty;

        if (!double.TryParse(bf, out var backFocusTolerance))
        { 
            backFocusTolerance = 0.001;
        }

        Console.Clear();

        var now = DateTime.Now;

        var solutions = await engine.SolveImageTrainAsync(
            target,
            sensor,
            connectors,
            backFocusTolerance,
            SolutionUpdate);
        
        var span = DateTime.Now - now;

        Console.Clear();
        Console.WriteLine($"Finished in {span}.");
        Console.Write("Press ENTER for the solutions.");
        Console.ReadLine();
        foreach(var solution in solutions)
        {
            if (barlow && !solution.Connections.Any(c => c.ComponentType == ComponentTypes.Barlow))
            {
                continue;
            }
            if (compression && !solution.Connections.Any(c => c.ComponentType == ComponentTypes.CompressionRing))
            {
                continue;
            }
            if (filterwheel && !solution.Connections.Any(c => c.ComponentType == ComponentTypes.FilterWheel))
            {
                continue;
            }
            Console.WriteLine(solution.ToString());
            Console.WriteLine();        
        }
    }
}

void SolutionUpdate(StatTracker tracker)
{
    if (DateTime.Now.Ticks - lastTick < refreshRateMs)
    {
        return;
    }
    Monitor.Enter(statusContext);
    try
    {
        Console.SetCursorPosition(1, 1);
        foreach (var type in StatTracker.AvailableResultTypes)
        {
            Console.Write($"{type}\t{tracker[type]}\t");
            Console.WriteLine();
        }
    }
    finally
    {
        Monitor.Exit(statusContext);
    }
    lastTick = DateTime.Now.Ticks;
}

