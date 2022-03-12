namespace AstroAdapt.Models
{
    /// <summary>
    /// Solution for problems.
    /// </summary>
    public class SolutionDomain
    {
        /// <summary>
        /// Gets or sets the available domain of solutions.
        /// </summary>
        public List<Solver> Solutions { get; set; } = new List<Solver>();

        /// <summary>
        /// Raised when the solution status changes;
        /// </summary>
        public event EventHandler<EventArgs> SolutionChanged;

        /// <summary>
        /// Gets or sets a value indicating whether solutions are being processed.
        /// </summary>
        public bool Solving { get; private set; } = false;

        /// <summary>
        /// Gets the total solutions in play.
        /// </summary>
        public int NumberSolutions => Solutions.Count;

        /// <summary>
        /// Gets the valid solutions.
        /// </summary>
        public int ValidSolutions => Solutions.Count(s => s.Solved);

        /// <summary>
        /// Solve the connections.
        /// </summary>
        /// <param name="inventory">Available adapters for solutions.</param>
        /// <param name="problems">The list of problems to solve for each inventory set.</param>
        public void Solve(IEnumerable<Component> inventory, params (Component target, Component sensor)[] problems)
        {
            if (Solving)
            {
                throw new InvalidOperationException("Can't start a new solution before the old one finishes.");
            }

            if (!inventory.Any() || !problems.Any())
            {
                return;
            }

            Solving = true;
        }
    }
}
