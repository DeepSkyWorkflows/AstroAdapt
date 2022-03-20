namespace AstroAdapt.Models
{
    /// <summary>
    /// An item of tracked resources.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Gets or sets the unique id of the component.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the component being tracked.
        /// </summary>
        public Component Component { get; set; } = null!;

        /// <summary>
        /// The number of items in hand.
        /// </summary>
        public int Qty { get; set; }
    }
}
