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
        /// Add a new item to inventory.
        /// </summary>
        /// <param name="newComponent">The new component.</param>
        /// <returns>The saved component.</returns>
        Task<Component> AddInventoryItemAsync(Component newComponent);

        /// <summary>
        /// Deletes an item from inventory.
        /// </summary>
        /// <param name="id">The id to delete.</param>
        /// <returns>The task.</returns>
        Task DeleteInventoryItemAsync(Guid id);

        /// <summary>
        /// Gets the related image info for the requested items.
        /// </summary>
        /// <param name="items">List of ids to resolve.</param>
        /// <returns>The image data.</returns>
        Task<IEnumerable<ImageData>> GetImagesForItemsAsync(IEnumerable<(Guid id, ComponentTypes type)> items);

        /// <summary>
        /// Sets the default image for a type.
        /// </summary>
        /// <param name="type">The type the default is for.</param>
        /// <param name="data">The image contents.</param>
        /// <returns>A task.</returns>
        Task SetDefaultImageAsync(ComponentTypes type, byte[] data);

        /// <summary>
        /// Sets the image for specific component.
        /// </summary>
        /// <param name="id">The id of the component the image is for.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="data">The image contents.</param>
        /// <returns>A task.</returns>
        Task SetImageAsync(Guid id, string filename, byte[] data);


        /// <summary>
        /// Query for solutions.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The solution list.</returns>
        Task<IEnumerable<SavedSolution>> LoadSolutionsAsync(
            Func<IQueryable<SavedSolution>, IQueryable<SavedSolution>>? query = null);

        /// <summary>
        /// Load a single solution.
        /// </summary>
        /// <param name="id">The solution id.</param>
        /// <returns>The solution.</returns>
        Task<SavedSolution?> LoadSolutionAsync(Guid id);

        /// <summary>
        /// Captures a snapshot of a solution and saves it.
        /// </summary>
        /// <param name="solution">The solution to save.</param>
        /// <param name="description">Description of the solution.</param>
        /// <returns>The <see cref="SavedSolution"/>.</returns>
        Task<SavedSolution> SaveSolutionAsync(
            Solution solution,
            string? description = null);

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
