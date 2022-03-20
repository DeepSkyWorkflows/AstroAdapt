using AstroAdapt.Data;
using AstroAdapt.Engine;
using AstroAdapt.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroAdapt.GraphQL
{
    public class Query
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Component> GetInventory([Service] IDbContextFactory<AstroContext> factory)
            => factory.CreateDbContext().Components;

        public async Task<IEnumerable<ImageResponse>> GetImagesAsync(
            [Service] IAstroApp astroApp,
            ImageRequest[] requestedImages)
            => (await astroApp.GetImagesForItemsAsync(requestedImages.Select(i => (i.Id, i.Type))))
            .Select(i => new ImageResponse(i));
    }
}
