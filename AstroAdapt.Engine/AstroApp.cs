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
            if (dbContextFactory == null)
            {
                throw new InvalidOperationException("InitializeAsync() must be called first!");
            }
            using var ctx = dbContextFactory.CreateDbContext();
            if (query == null)
            {
                return await ctx.Components.ToListAsync();
            }
            return await query(ctx.Components).ToListAsync();
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

        private void Sd_SolutionChanged(object? sender, SolutionEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
