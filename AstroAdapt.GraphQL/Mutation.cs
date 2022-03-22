using AstroAdapt.Engine;
using AstroAdapt.Models;
using HotChocolate.Resolvers;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// GraphQL operations with side effects.
    /// </summary>
    public class Mutation
    {
        /// <summary>
        /// Save a solution.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="context">The context.</param>
        /// <param name="astroService">The app service.</param>
        /// <returns>The saved solution.</returns>
        public async Task<SavedSolution> SaveSolutionAsync(
            Solution solution,
            [Service]IAstroApp astroService) =>
            await astroService.SaveSolutionAsync(solution);                    

        /// <summary>
        /// Delete a solution.
        /// </summary>
        /// <param name="savedSolutionId">The id of the solution.</param>
        /// <param name="astroService">The app service.</param>
        /// <returns>A value indicating whether the delete was successful.</returns>
        public async Task<bool> DeleteSolutionAsync(
            Guid savedSolutionId,
            [Service] IAstroApp astroService)
        {
            await astroService.DeleteSolutionAsync(savedSolutionId);
            return true;
        }
    }
}
