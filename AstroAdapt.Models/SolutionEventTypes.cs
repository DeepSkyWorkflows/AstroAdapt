namespace AstroAdapt.Models
{
    /// <summary>
    /// Events for the solution engine.
    /// </summary>
    public enum SolutionEventTypes
    {
        /// <summary>
        /// A new solution workload was spawned.
        /// </summary>
        SolverSpawned,

        /// <summary>
        /// A new solution was found.
        /// </summary>
        SolutionFound,

        /// <summary>
        /// The path ended without a solution.
        /// </summary>
        SolutionFailed,

        /// <summary>
        /// All solvers have finished.
        /// </summary>
        SolvingFinished
    }
}
