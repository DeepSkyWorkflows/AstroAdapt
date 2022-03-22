using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Message to update a solution-in-progress.
    /// </summary>
    public class SolutionProcessingUpdate
    {
        /// <summary>
        /// Id of the request.
        /// </summary>
        public long CorrelationId { get; set; }

        /// <summary>
        /// The stats.
        /// </summary>
        public Dictionary<SolverResults, long> Statistics { get; set; } =
            new Dictionary<SolverResults, long>();

        /// <summary>
        /// The args.
        /// </summary>
        public SolutionEventArgs? Args { get; set; } = null;
    }
}
