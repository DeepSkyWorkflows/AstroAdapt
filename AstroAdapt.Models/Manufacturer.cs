namespace AstroAdapt.Models
{
    /// <summary>
    /// Manufacturer of components
    /// </summary>
    public class Manufacturer
    {
        /// <summary>
        /// Gets or sets the unique identifier for the manufacturer.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the manufacturuer name.
        /// </summary>
        public string Name { get; set; } = "Generic";

        /// <summary>
        /// Gets or sets the manufacturer home page
        /// </summary>
        public Uri? Homepage { get; set; } = null;

        /// <summary>
        /// List of components by this manufacturer
        /// </summary>
        public List<Component> Components { get; private set; } = new List<Component>();
    }
}
