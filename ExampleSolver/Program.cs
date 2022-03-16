// See https://aka.ms/new-console-template for more information

using AstroAdapt.Models;

var refreshRateMs = TimeSpan.FromMilliseconds(1000).Ticks;
var lastTick = DateTime.Now.Ticks;

const string FilePath = @"Data/SampleInventory.csv";
Console.WriteLine("Example solver.");

if (!File.Exists(FilePath))
{
    Console.WriteLine("Sample inventory file not found.");
    return;
}

Console.WriteLine("Parsing inventory...");
var componentSrc = File.ReadAllLines(FilePath);

// Manufacturer,Model,Type,Target Type,Target Size,Sensor Type,Sensor Size,BF,Length,Threads,Reversible,InsertionPoint,Qty
// ZWO,ASI294MCPro,Sensor,Inserter,M42 ,Terminator,Zero,0,0,6.5,FALSE,FlushToSensor,1

var componentList = new List<Component>();
var header = true;

var manufacturers = new List<Manufacturer>();

Manufacturer getManufacturer(string m)
{
    var result = manufacturers.FirstOrDefault(manufacturer => manufacturer.Name == m);
    if (result == null)
    {
        result = new Manufacturer { Name = m };
        manufacturers.Add(result);
    }
    return result;
}

foreach (var componentCols in componentSrc)
{
    if (header)
    {
        header = false;
        continue;
    }
    var columns = componentCols.Split(',');
    var (
        manufacturer,
        model,
        type,
        targetType,
        targetSize,
        sensorType,
        sensorSize,
        backFocus,
        length,
        threads,
        reversible,
        insertPoint,
        qty) = (
        columns[0],
        columns[1],
        columns[2],
        columns[3],
        columns[4],
        columns[5],
        columns[6],
        columns[7],
        columns[8],
        columns[9],
        columns[10],
        columns[11],
        columns[12]);
    Component component = new()
    {
        BackFocusMm = double.Parse(backFocus),
        ComponentType = Enum.Parse<ComponentTypes>(type),
        InsertionPoint = Enum.Parse<InsertionPoints>(insertPoint),
        IsReversible = bool.Parse(reversible),
        LengthMm = double.Parse(length),
        Manufacturer = getManufacturer(manufacturer),
        Model = model,
        SensorDirectionConnectionSize = Enum.Parse<ConnectionSizes>(sensorSize),
        SensorDirectionConnectionType = Enum.Parse<ConnectionTypes>(sensorType),
        TargetDirectionConnectionSize = Enum.Parse<ConnectionSizes>(targetSize),
        TargetDirectionConnectionType = Enum.Parse<ConnectionTypes>(targetType),
        ThreadRecessMm = double.Parse(threads)
    };
    if (component.Manufacturer.Name.StartsWith("^"))
    {
        continue;
    }
    var items = int.Parse(qty);
    while (items-- > 0)
    {
        componentList.Add(component.Clone());
    }
    Console.WriteLine(component);
}

var targets = componentList.Where(c => c.TargetDirectionConnectionType == ConnectionTypes.Terminator);
var sensors = componentList.Where(c => c.SensorDirectionConnectionType == ConnectionTypes.Terminator);
var connectors = componentList.Where(c => c.SensorDirectionConnectionType != ConnectionTypes.Terminator
 && c.TargetDirectionConnectionType != ConnectionTypes.Terminator);

foreach (var target in targets)
{
    foreach (var sensor in sensors)
    {
        Console.WriteLine($"Solving image train from {target} to {sensor}.");
        Console.WriteLine($"s=skip,b=barlow,f=filterwheel,c=compressionring");
        Console.Write("Enter commands: ");
        var response = Console.ReadLine() ?? string.Empty;
        response = response.Trim().ToLowerInvariant();
        if (response == "s")
        {
            continue;
        }
        Predicate<IEnumerable<Component>>? filter = null;
        if (response.ToCharArray().Intersect(new[] { 'b', 'f', 'c' }).Any())
        {
            if (response.Contains('b'))
            {
                filter = inv => inv.Any(c => c.ComponentType == ComponentTypes.Barlow);
            }
            if (response.Contains('f'))
            {
                Predicate<IEnumerable<Component>> efw =
                    inv => inv.Any(c => c.ComponentType == ComponentTypes.FilterWheel);

                if (filter == null)
                {
                    filter = efw;
                }
                else
                {
                    filter = inv => filter(inv) && efw(inv);
                }
            }
            if (response.Contains('c'))
            {
                Predicate<IEnumerable<Component>> ring =
                    inv => inv.Any(c => c.ComponentType == ComponentTypes.CompressionRing);

                if (filter == null)
                {
                    filter = ring;
                }
                else
                {
                    filter = inv => filter(inv) && ring(inv);
                }
            }
        }
        var sd = new SolutionDomain(12);
        Console.Clear();
        sd.SolutionChanged += Sd_SolutionChanged;
        var now = DateTime.Now; 
        await sd.SolveAsync(connectors, target, sensor, 0.05, filter);
        var span = DateTime.Now - now;
        Console.WriteLine($"Created {sd.ValidSolutions} in {span}.");
        Console.Write("Press ENTER for the solutions.");
        Console.ReadLine();
        Console.Clear();
        var best = sd.GetSolutions();
        foreach (var solution in best)
        {
            Console.WriteLine(solution.ToString());
        }
    }
}

void Sd_SolutionChanged(object? sender, SolutionEventArgs e)
{
    if (e.EventType == SolutionEventTypes.SolutionFailed &&
        DateTime.Now.Ticks - lastTick < refreshRateMs)
    {
        return;
    }
    var wide = new string(' ', Console.BufferWidth - 1);
    Console.SetCursorPosition(0, 0);
    Console.WriteLine(string.Join(Environment.NewLine, new[] { wide, wide, wide, wide, wide, wide, wide }));
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"Event: {e.EventType}");
    Console.WriteLine($"{e.TotalSolvers} solutions in process, {e.TotalSolutions} valid.");
    if (e.EventType == SolutionEventTypes.SolverSpawned)
    {
        Console.WriteLine($"Solution size: {e.Solver?.SolutionSize}");
    }
    else if (e.EventType == SolutionEventTypes.SolutionFound)
    {
        Console.WriteLine($"{e.Solution}");
    }
    else if (e.EventType == SolutionEventTypes.SolvingFinished)
    {
        Console.WriteLine("Top 3 solved:");
        foreach (var solution in e.Solutions!)
        {
            Console.WriteLine(solution.ToString());
        }
    }
    lastTick = DateTime.Now.Ticks;
}
