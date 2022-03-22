using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Final message when solution is solved.
    /// </summary>
    public class FinalSolution
    {
        /// <summary>
        /// Unique id to correlate messages.
        /// </summary>
        public long CorrelationId { get; set; }

        /// <summary>
        /// The list of available solutions.
        /// </summary>
        public List<Solution> Solutions { get; set; } = new List<Solution>();
    }
}
