using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Queries for the adapters.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Gets all available inventory.
        /// </summary>
        /// <param name="context">The DbContext.</param>
        /// <returns>The list of inventory.</returns>
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Component> GetInventory(AstroContext context)
            => context.Components;

        /// <summary>
        /// Gets all manufacturers.
        /// </summary>
        /// <param name="context">The DbContext.</param>
        /// <returns>The list of manufacturers.</returns>
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Manufacturer> GetManufacturers(AstroContext context)
            => context.Manufacturers;

        /// <summary>
        /// Get image bytes for display.
        /// </summary>
        /// <param name="astroApp">The app engine.</param>
        /// <param name="requestedImages">The list of images to resolve.</param>
        /// <returns>The images.</returns>
        public async Task<IEnumerable<ImageResponse>> GetImagesAsync(
            IAstroApp astroApp,
            ImageRequest[] requestedImages)
            => (await astroApp.GetImagesForItemsAsync(requestedImages.Select(i => (i.Id, i.Type))))
            .Select(i => new ImageResponse(i));

        /// <summary>
        /// Get a unique id for a solution. This will only queue the solution. You must subscribe
        /// to the "ProblemSolved" topic with the id to start the solver.
        /// </summary>
        /// <param name="queue">Processing service.</param>
        /// <param name="targetId">Id of target.</param>
        /// <param name="sensorId">Id of sensor.</param>
        /// <param name="components">Components for solution.</param>
        /// <param name="backFocusTolerance">Percent tolerance from back focus.</param>
        /// <returns>A unique id you can use to subscribe for results.</returns>
        public long GetSolutionSubscriptionId(
            [Service] SolutionProcessingService queue,
            Guid targetId,
            Guid sensorId,
            List<Guid> components,
            double backFocusTolerance) =>
            queue.QueueSolution(
                targetId,
                sensorId,
                components,
                backFocusTolerance);

        /// <summary>
        /// Get the available options for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The options.</returns>
        public IEnumerable<string> GetOptionsForType(
            string type) => type switch
            {
                nameof(ComponentTypes) => Enum.GetNames(typeof(ComponentTypes)),
                nameof(InsertionPoints) => Enum.GetNames(typeof(InsertionPoints)),
                nameof(ConnectionTypes) => Enum.GetNames(typeof(ConnectionTypes)),
                nameof(ConnectionSizes) => Enum.GetNames(typeof(ConnectionSizes)),
                _ => Array.Empty<string>(),
            };

        /// <summary>
        /// Gets all saved solutions.
        /// </summary>
        /// <param name="context">The DbContext.</param>
        /// <returns>The list of solutions.</returns>
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<SavedSolution> GetSavedSolutions(AstroContext context)
            => context.Solutions;

    }
}

