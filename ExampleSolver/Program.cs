// See https://aka.ms/new-console-template for more information

using AstroAdapt.Models;

var refreshRateMs = TimeSpan.FromMilliseconds(1000).Ticks;
var lastTick = DateTime.Now.Ticks;
object statusContext = new();

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
        var newItem = component.Clone();
        newItem.Id = Guid.NewGuid();
        componentList.Add(newItem);
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

        var sd = new SolutionDomain(12);

        Console.Clear();

        var statTracker = new StatTracker(sd, SolutionUpdate);
        var now = DateTime.Now;

        await sd.SolveAsync(connectors, target, sensor, backFocusTolerance);

        var span = DateTime.Now - now;

        Console.Clear();
        Console.WriteLine($"Finished in {span}.");
        foreach (var type in StatTracker.AvailableResultTypes)
        {
            Console.WriteLine($"{type}\t\t{statTracker[type]}");
            Console.WriteLine();
        }
        Console.Write("Press ENTER for the solutions.");
        Console.ReadLine();
        var query = sd.GetSolutions();
        foreach(var solution in query)
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

