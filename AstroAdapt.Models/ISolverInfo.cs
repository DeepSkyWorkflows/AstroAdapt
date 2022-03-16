namespace AstroAdapt.Models
{
    /// <summary>
    /// Info about the solver.
    /// </summary>
    public interface ISolverInfo
    {
        /// <summary>
        /// Gets the connections in this solution path.
        /// </summary>
        Connector Connections { get; }

        /// <summary>
        /// Gets the sensor.
        /// </summary>
        Component Sensor { get; }

        /// <summary>
        /// Gets the unique signature.
        /// </summary>
        byte[] Signature { get; }

        /// <summary>
        /// Gets the number of items being considered.
        /// </summary>
        int SolutionSize { get; }

        /// <summary>
        /// Gets a value that indicates whether it was solved.
        /// </summary>
        bool Solved { get; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        Component Target { get; }
    }
}
