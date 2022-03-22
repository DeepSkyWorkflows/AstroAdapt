using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Request an image.
    /// </summary>
    public class ImageRequest
    {
        /// <summary>
        /// The id of the component. 
        /// </summary>
        /// <remarks>Set to default (all 0s) to get the generic component type image.</remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of the component.
        /// </summary>
        public ComponentTypes Type { get; set; }
    }
}
