namespace AstroAdapt.Models
{
    /// <summary>
    /// A saved combination.
    /// </summary>
    public class SavedSolution
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target for solution.
        /// </summary>
        public SolutionItem Target { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Sensor.
        /// </summary>
        public SolutionItem Sensor { get; set; } = null!;

        /// <summary>
        /// Ordered list of solution items.
        /// </summary>
        public List<SolutionItem> Items { get; set; } = new List<SolutionItem>(); 
    }
}
