using AstroAdapt.Models;

namespace AstroAdapt.Data
{
    /// <summary>
    /// Single-purpose class to seed sample data.
    /// </summary>
    public static class AstroSeed
    {
        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <param name="astroContext">The context to use.</param>
        /// <returns>A value indicating whether the seed was successful.</returns>
        public static async Task<bool> SeedDatabaseAsync(AstroContext astroContext)
        {
            const string FilePath = @"../../../../ExampleSolver/Data/SampleInventory.csv";

            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("Sample inventory file not found.");
            }

            var componentSrc = await File.ReadAllLinesAsync(FilePath);

            // Manufacturer,Model,Type,Target Type,Target Size,Sensor Type,Sensor Size,BF,Length,Threads,Reversible,InsertionPoint,Qty
            // ZWO,ASI294MCPro,Sensor,Inserter,M42 ,Terminator,Zero,0,0,6.5,FALSE,FlushToSensor,1

            var header = true;

            var manufacturers = new List<Manufacturer>();

            Manufacturer getManufacturer(string m, AstroContext ctx)
            {
                var result = manufacturers.FirstOrDefault(manufacturer => manufacturer.Name == m);
                if (result == null)
                {
                    result = new Manufacturer { Name = m };
                    manufacturers.Add(result);
                    ctx.Manufacturers.Add(result);
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
                    qty,
                    shortcode) = (
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
                    columns[12],
                    columns[13]);
                if (manufacturer.StartsWith("^"))
                {
                    continue;
                }
                Component component = new()
                {
                    BackFocusMm = double.Parse(backFocus),
                    ComponentType = Enum.Parse<ComponentTypes>(type),
                    InsertionPoint = Enum.Parse<InsertionPoints>(insertPoint),
                    IsReversible = bool.Parse(reversible),
                    LengthMm = double.Parse(length),
                    Manufacturer = getManufacturer(manufacturer, astroContext),
                    Model = model,
                    SensorDirectionConnectionSize = Enum.Parse<ConnectionSizes>(sensorSize),
                    SensorDirectionConnectionType = Enum.Parse<ConnectionTypes>(sensorType),
                    TargetDirectionConnectionSize = Enum.Parse<ConnectionSizes>(targetSize),
                    TargetDirectionConnectionType = Enum.Parse<ConnectionTypes>(targetType),
                    ThreadRecessMm = double.Parse(threads),
                    ShortCode = columns[13]
                };
                var items = int.Parse(qty);
                while (items-- > 0)
                {
                    var newItem = component.Clone();
                    newItem.Id = Guid.NewGuid();
                    newItem.Manufacturer.Components.Add(newItem);
                    astroContext.Components.Add(newItem);
                }
            }
            await astroContext.SaveChangesAsync();
            return true;
        }
    }
}
