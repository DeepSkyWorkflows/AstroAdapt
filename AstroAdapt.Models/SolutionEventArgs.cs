﻿namespace AstroAdapt.Models
{
    /// <summary>
    /// Events generated by the solver. 
    /// </summary>
    public class SolutionEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SolutionEventArgs"/> class.
        /// </summary>
        /// <param name="sd">The solution domain raising the event.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="solverResult">The solver result.</param>
        public SolutionEventArgs(
            SolutionDomain sd,
            SolutionEventTypes eventType,
            SolverResults solverResult = SolverResults.Info)
        {
            TotalSolvers = sd.NumberSolutions;
            TotalSolutions = sd.ValidSolutions;
            EventType = eventType;
            SolverResult = solverResult;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SolutionEventArgs"/> class.
        /// </summary>
        /// <param name="sd">The solution domain raising the event.</param>
        /// <param name="solution">The <see cref="Solution"/> that was created.</param>
        public SolutionEventArgs(SolutionDomain sd, Solution solution) :
            this(sd, SolutionEventTypes.SolutionFound)
        {
            payload = solution;
            SolverResult = SolverResults.Solved;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SolutionEventArgs"/> class.
        /// </summary>
        /// <param name="sd">The solution domain raising the event.</param>
        /// <param name="solutions">The list <see cref="Solution"/> instances found.</param>
        public SolutionEventArgs(SolutionDomain sd, IEnumerable<Solution> solutions) :
            this(sd, SolutionEventTypes.SolvingFinished) => payload = solutions;

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public SolutionEventTypes EventType { get; private set; }

        /// <summary>
        /// Number of solutions actively queued.
        /// </summary>
        public int TotalSolvers { get; private set; }

        /// <summary>
        /// Gets the count of successful solutions.
        /// </summary>
        public int TotalSolutions { get; private set; }

        /// <summary>
        /// Payload when applicatable.
        /// </summary>
        private readonly object? payload;

        /// <summary>
        /// Reason to trigger the update.
        /// </summary>
        public SolverResults SolverResult { get; private set; } = SolverResults.Info;

        /// <summary>
        /// Gets the solution.
        /// </summary>
        public Solution? Solution => payload as Solution;

        /// <summary>
        /// Gets all solutions.
        /// </summary>
        public IEnumerable<Solution>? Solutions => payload as IEnumerable<Solution>;
    }
}
