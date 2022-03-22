using AstroAdapt.Engine;
using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Extend component to enable fetching the image.
    /// </summary>
    public class ComponentExtensions : ObjectTypeExtension<Component>
    {
        /// <summary>
        /// Configures the extensions.
        /// </summary>
        /// <param name="descriptor">The descriptor for components.</param>
        protected override void Configure(IObjectTypeDescriptor<Component> descriptor)
        {
            descriptor
                .Field("image")
                .Type<ObjectType<ImageData>>()
                .Resolve(async ctx =>
                {
                    var astroService = ctx.Service<IAstroApp>();
                    var component = ctx.Parent<Component>();
                    return await GetImageAsync(component, astroService);
                });            
        }

        /// <summary>
        /// Fetch the image for the component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="astroService">The service.</param>
        /// <returns>The image data.</returns>
        private static async Task<ImageData?> GetImageAsync(
            Component component,
            IAstroApp astroService) =>
            (await astroService.GetImagesForItemsAsync(
                new[] { (component.Id, component.ComponentType) }))
            .FirstOrDefault();
    }
}
