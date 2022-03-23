using AstroAdapt.Engine;
using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Extend component to enable fetching the image.
    /// </summary>
    [ExtendObjectType(typeof(Component))]
    public class ComponentExtensions
    {
        public async Task<ImageData?> GetImageAsync([Parent] Component component, IAstroApp astroApp)
        {
            return (await astroApp.GetImagesForItemsAsync(
                new[] { (component.Id, component.ComponentType) }))
                    .FirstOrDefault();
        }
    }
}
