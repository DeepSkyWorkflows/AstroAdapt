using AstroAdapt.Data;
using AstroAdapt.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroAdapt.Engine
{
    /// <summary>
    /// Interface for application actions.
    /// </summary>
    public interface IAstroApp
    {
        /// <summary>
        /// Initialize everything.
        /// </summary>
        /// <param name="resolveFactory">Given the database path, generate the context factory.</param>
        /// <param name="rootPath">The root path. If not specified, app data will be used.</param>
        /// <param name="forceReset">True to wipe and reload database.</param>
        Task InitializeAsync(
            Func<string, IDbContextFactory<AstroContext>> resolveFactory,
            string? rootPath = null,
            bool forceReset = false);

        /// <summary>
        /// Query for components.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The component list.</returns>
        Task<IEnumerable<Component>> LoadInventoryAsync(
            Func<IQueryable<Component>, IQueryable<Component>>? query = null);

        /// <summary>
        /// Requests a solution.
        /// </summary>
        /// <param name="target">The target lens to solve.</param>
        /// <param name="sensor">The sesnor to solve.</param>
        /// <param name="adapters">The available adapters.</param>
        /// <param name="backFocusTolerance">The back focus tolerance.</param>
        /// <param name="statsCallback">Receive pure stat updates.</param>
        /// <param name="solutionUpdate">Receive solution progress updates.</param>
        /// <param name="workerCount">null for auto, 0 for synchronous, anything else will asynchronously run the work count passed.</param>
        /// <returns>The final solution list.</returns>
        Task<IEnumerable<Solution>> SolveImageTrainAsync(
            Component target,
            Component sensor,
            IEnumerable<Component> adapters,
            double backFocusTolerance = 0.01,
            Action<StatTracker>? statsCallback = null,
            Action<SolutionEventArgs>? solutionUpdate = null,
            int? workerCount = null);

        /// <summary>
        /// Gets or sets the path to images.
        /// </summary>
        string PathToImages { get; }

        /// <summary>
        /// Gets the path to the database.
        /// </summary>
        string PathToDatabase { get; }

    }
}
