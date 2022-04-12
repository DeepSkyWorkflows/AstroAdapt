// See https://aka.ms/new-console-template for more information

using System.Text;
using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.Models;
using ExampleSolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var refreshRateMs = TimeSpan.FromMilliseconds(1000).Ticks;
var lastTick = DateTime.Now.Ticks;
object statusContext = new();
Solution? lastSolution = null; 

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
var solutionsToSave = new List<Solution>();

foreach (var target in targets)
{
    bool skipTarget = false;
    foreach (var sensor in sensors)
    {
        if (skipTarget)
        {
            continue;
        }

        Console.WriteLine($"Solving image train from {target} to {sensor}.");
        Console.WriteLine($"a=skip target,s=skip,i=inventory");
        Console.Write("Enter commands: ");
        var response = Console.ReadLine() ?? string.Empty;
        response = response.Trim().ToLowerInvariant();
        var selectedConnectors = new List<Component>(connectors);
        if (response == "s")
        {
            continue;
        }
        if (response == "a")
        {
            skipTarget = true;
            continue;
        }
        if (response == "i")
        {
            Console.WriteLine("ENTER to keep, x to exclude");
            foreach (var c in connectors)
            {
                Console.Write($"{c}:");
                var resp = Console.ReadLine() ?? string.Empty;
                if (resp.ToLowerInvariant().Trim() == "x")
                {
                    selectedConnectors.Remove(c);
                }
                Console.WriteLine();
            }
        }
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
            selectedConnectors,
            backFocusTolerance,
            SolutionUpdate,
            GrabLastSolution);

        var span = DateTime.Now - now;

        Console.Clear();
        Console.WriteLine($"Finished in {span} with {solutions.Count()} solutions.");
        Console.WriteLine($"filter (prefix with 'n' to exclude) b=barlow,c=compresion ring,f=filter wheel");
        Console.Write("Enter commands: ");
        response = Console.ReadLine() ?? string.Empty;
        response = response.Trim().ToLowerInvariant();
        var nocompression = response.Contains("nc");
        var compression = !nocompression && response.Contains('c');
        var nobarlow = response.Contains("nb");
        var barlow = !nobarlow && response.Contains('b');
        var nofilterwheel = response.Contains("nf");
        var filterwheel = !nofilterwheel && response.Contains('f');
        var query = solutions.AsQueryable();

        if (nocompression)
        {
            query = query.Where(s => !s.Connections.Any(c => c.ComponentType == ComponentTypes.CompressionRing));
        }
        if (compression)
        {
            query = query.Where(s => s.Connections.Any(c => c.ComponentType == ComponentTypes.CompressionRing));
        }

        if (nobarlow)
        {
            query = query.Where(s => !s.Connections.Any(c => c.ComponentType == ComponentTypes.Barlow));
        }
        if (barlow)
        {
            query = query.Where(s => s.Connections.Any(c => c.ComponentType == ComponentTypes.Barlow));
        }

        if (nofilterwheel)
        {
            query = query.Where(s => !s.Connections.Any(c => c.ComponentType == ComponentTypes.FilterWheel));
        }
        if (filterwheel)
        {
            query = query.Where(s => s.Connections.Any(c => c.ComponentType == ComponentTypes.FilterWheel));
        }

        var sortedQuery = query
            .OrderByDescending(s => s.Weight)
            .ThenBy(s => s.Deviance)
            .ThenBy(s => s.ComponentCount);
        var pageIdx = 0;
        var perfect = sortedQuery.Count(s => s.Deviance == 0);
        Console.WriteLine($"Filtered to {sortedQuery.Count()} solutions, {perfect} are perfect.");
        if (perfect > 0)
        {
            Console.Write("Type 'p' to only show perfect solutions: ");
            response = Console.ReadLine() ?? string.Empty;
            if (response.ToLowerInvariant().Contains('p'))
            {
                sortedQuery = sortedQuery.Where(s => s.Deviance == 0) as IOrderedQueryable<Solution>;
            }
        }
        var solIdx = 0;
        var page = new List<Solution>();
        foreach (var solution in sortedQuery!)
        {
            page.Add(solution);
            Console.WriteLine($"{pageIdx++} {solIdx++} {solution.ToShortString()}");
            Console.WriteLine();
            if (pageIdx == 5)
            {
                pageIdx = 0;
                Console.Write("ENTER for next page, 'a' to save all, numbers to save, 's' to skip to next: ");
                response = (Console.ReadLine() ?? string.Empty).ToLowerInvariant();
                if (response.Contains('s'))
                {
                    page.Clear();
                    break;
                }
                if (response.Contains('a'))
                {
                    solutionsToSave.AddRange(page);                    
                }
                else
                {
                    for (var idx = 0; idx < page.Count; idx++)
                    {
                        if (response.Contains(idx.ToString()))
                        {
                            solutionsToSave.Add(page[idx]);
                        }
                    }                    
                }
                page.Clear();
            }

        }
        if (page.Count > 0)
        {
            Console.Write("Last page: 'a' to save all, numbers to save: ");
            response = (Console.ReadLine() ?? string.Empty).ToLowerInvariant();
            if (response.Contains('a'))
            {
                solutionsToSave.AddRange(page);
            }
            else
            {
                for (var idx = 0; idx < page.Count; idx++)
                {
                    if (response.Contains(idx.ToString()))
                    {
                        solutionsToSave.Add(page[idx]);
                    }
                }
            }
        }        
    }    
}

if (solutionsToSave.Count > 0)
{
    Console.WriteLine($"{solutionsToSave.Count} saved solutions.");
    Console.Write("d=display, s=save, any other=exit: ");

    var response = (Console.ReadLine() ?? string.Empty).ToLowerInvariant();

    if (!response.Contains('d') && !response.Contains('s'))
    {
        return;
    }

    var sb = new StringBuilder();
    sb.AppendLine("SUMMARY");
    foreach (var solution in solutionsToSave)
    {
        sb.AppendLine(string.Join(" -> ", solution.Connections.Select(c => c.ShortCode ?? c.Model).ToArray()));
    }
    sb.AppendLine();
    sb.AppendLine(" === ");
    sb.AppendLine();
    sb.AppendLine("DETAIL");
    foreach (var solution in solutionsToSave)
    {
        sb.Append(new FormatHelper(solution).Build());
    }
    var instructions = sb.ToString();
    if (response.Contains('d'))
    {
        Console.WriteLine(instructions);
    }
    if (response.Contains('s'))
    {
        var idx = 0;
        var found = true;
        string filePath = string.Empty;
        do
        {
            var filename = idx++ == 0 ? "solutions.txt" : $"solutions{idx}.txt";
            filePath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal), filename);
            found = File.Exists(filePath);
        }
        while (found);
        File.WriteAllText(filePath, instructions);
        Console.WriteLine($"Solutions saved to {filePath}");
    }
}

void GrabLastSolution(SolutionEventArgs e)
{
    if (e.Solution != null)
    {
        lastSolution = e.Solution;
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
        Console.WriteLine("Press any key to terminate");
        foreach (var type in StatTracker.AvailableResultTypes)
        {
            Console.Write($"{type}\t{tracker[type]}\t");
            Console.WriteLine();
        }
        if (lastSolution != null)
        {
            Console.WriteLine(lastSolution.ToShortString());
        }
    }
    finally
    {
        Monitor.Exit(statusContext);        
    }
    if (Console.KeyAvailable)
    {
        engine.Terminate();
    }
    lastTick = DateTime.Now.Ticks;
}

