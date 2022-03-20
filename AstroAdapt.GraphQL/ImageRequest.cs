using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    public class ImageRequest
    {
        public Guid Id { get; set; }
        public ComponentTypes Type { get; set; }
    }
}
