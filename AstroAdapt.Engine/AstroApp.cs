using AstroAdapt.Data;
using AstroAdapt.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroAdapt.Engine
{
    /// <summary>
    /// Application engine.
    /// </summary>
    public class AstroApp : IAstroApp
    {
        private IDbContextFactory<AstroContext>? dbContextFactory = null;

        /// <summary>
        /// Gets the path to images.
        /// </summary>
        public string PathToImages { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the path to the database.
        /// </summary>
        public string PathToDatabase { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public async Task<Component> AddInventoryItemAsync(Component newComponent)
        {
            using var ctx = GetContext();
            ctx.Components.Add(newComponent);
            await ctx.SaveChangesAsync();
            return newComponent;
        }

        /// <inheritdoc/>
        public async Task DeleteInventoryItemAsync(Guid id)
        {
            using var ctx = GetContext();
            var componentToDelete = await ctx.Components.SingleAsync(c => c.Id == id);
            ctx.Components.Remove(componentToDelete);
            await ctx.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ImageData>> GetImagesForItems(
            IEnumerable<(Guid id, ComponentTypes type)> items)
        {
            var result = new List<ImageData>();
            var files = Directory.EnumerateFiles(PathToImages).ToList();
            foreach(var (id, type) in items)
            {
                var file = files.FirstOrDefault(f => f.Contains(id.ToString())) ??
                    Path.Combine(PathToImages, $"{type}.png");
                var image = new ImageData
                {
                    Id = id,
                    FileName = Path.GetFileName(file),
                    Image = await File.ReadAllBytesAsync(file)
                };
                result.Add(image);            
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            Func<string, IDbContextFactory<AstroContext>> resolveFactory,
            string? rootPath = null,
            bool forceReset = false)
        {
            // setup paths
            rootPath ??= Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            rootPath = Path.Combine(rootPath, "astro-app");
            PathToImages = Path.Combine(rootPath, @"astro-app-images");
            PathToDatabase = Path.Combine(rootPath, @"data\astroapp.sqlite");

            if (!Directory.Exists(PathToImages))
            {
                Directory.CreateDirectory(PathToImages);
                foreach (var componentType in Enum.GetValues<ComponentTypes>())
                {
                    var src = $"DefaultImages/{componentType}.png";
                    var tgt = Path.Combine(PathToImages, $"{componentType.ToString().ToLowerInvariant()}.png");
                    File.Copy(src, tgt, true);
                }
            }

            if (!Directory.Exists(Path.GetDirectoryName(PathToDatabase)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PathToDatabase)!);
            }

            dbContextFactory = resolveFactory(PathToDatabase);

            using var ctx = dbContextFactory.CreateDbContext();

            if (forceReset)
            {
                await ctx.Database.EnsureDeletedAsync();
            }

            if (await ctx.Database.EnsureCreatedAsync())
            {
                await AstroSeed.SeedDatabaseAsync(ctx);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Component>> LoadInventoryAsync(
            Func<IQueryable<Component>, IQueryable<Component>>? query = null)
        {
            using var ctx = GetContext();
            if (query == null)
            {
                return await ctx.Components.ToListAsync();
            }
            return await query(ctx.Components).ToListAsync();
        }        

        /// <inheritdoc/>
        public async Task<SavedSolution?> LoadSolutionAsync(Guid id)
        {
            using var ctx = GetContext();
            return await ctx.Solutions.Where(s => s.Id == id)
                .Include(s => s.Sensor)
                .Include(s => s.Target)
                .Include(s => s.Items)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SavedSolution>> LoadSolutionsAsync(
            Func<IQueryable<SavedSolution>,
                IQueryable<SavedSolution>>? query = null)
        {
            using var ctx = GetContext();
            if (query == null)
            {
                return await ctx.Solutions.ToListAsync();
            }
            return await query(ctx.Solutions).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<SavedSolution> SaveSolutionAsync(Solution solution, string? description = null)
        {
            var savedSolution = solution.ToSavedSolution();
            savedSolution.Description = description ?? string.Empty;
            using var ctx = GetContext();
            ctx.Solutions.Add(savedSolution);
            await ctx.SaveChangesAsync();
            return savedSolution;
        }

        /// <inheritdoc/>
        public async Task SetDefaultImageAsync(ComponentTypes type, byte[] data)
        {
            var file = Path.Combine(PathToImages, $"{type}.png");
            await File.WriteAllBytesAsync(file, data);
        }

        /// <inheritdoc/>
        public async Task SetImageAsync(Guid id, string filename, byte[] data)
        {
            var ext = Path.GetExtension(filename);
            var targetFile = Path.Combine(PathToImages, $"{id}{ext}");
            await File.WriteAllBytesAsync(targetFile, data);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Solution>> SolveImageTrainAsync(
            Component target,
            Component sensor,
            IEnumerable<Component> adapters,
            double backFocusTolerance = 0.01,
            Action<StatTracker>? statsCallback = null,
            Action<SolutionEventArgs>? solutionUpdate = null,
            int? workerCount = null)
        {
            if (workerCount == null)
            {
                workerCount = Environment.ProcessorCount;
            }

            var sd = new SolutionDomain(workerCount.Value);

            StatTracker? statTracker = null;

            if (statsCallback != null)
            {
                statTracker = new StatTracker(sd, statsCallback);
            }

            EventHandler<SolutionEventArgs>? handler = null;

            if (solutionUpdate != null)
            {
                handler = (o, e) => solutionUpdate(e);
                sd.SolutionChanged += handler;
            }

            await sd.SolveAsync(adapters, target, sensor, backFocusTolerance);

            if (statTracker != null)
            {
                statTracker.Dispose();
            }

            if (handler != null)
            {
                sd.SolutionChanged -= handler;
            }

            return sd.GetSolutions();
        }

        private AstroContext GetContext()
        {
            if (dbContextFactory == null)
            {
                throw new InvalidOperationException("AstroApp not initialized. Call InitializeAsync() first.");
            }

            return dbContextFactory.CreateDbContext();
        }
    }
}
