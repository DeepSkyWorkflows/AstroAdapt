namespace AstroAdapt.Models
{
    /// <summary>
    /// Results from a pass through the solver.
    /// </summary>
    public enum SolverResults
    {
        /// <summary>
        /// No change in status.
        /// </summary>
        Info,

        /// <summary>
        /// Finished due to forking for further solutions.
        /// </summary>
        Forked,

        /// <summary>
        /// No more compatible parts.
        /// </summary>
        DeadEnd,

        /// <summary>
        /// Used all of the parts available to connect to the sensor.
        /// </summary>
        NoSensorConnection,

        /// <summary>
        /// Back focus is unacceptable length.
        /// </summary>
        OutsideTolerance,

        /// <summary>
        /// A duplication solution exists.
        /// </summary>
        Duplicate,

        /// <summary>
        /// A cancellation request was sent.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The problem was solved within tolerances.
        /// </summary>
        Solved,
    }
}
