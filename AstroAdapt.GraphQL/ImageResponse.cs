using AstroAdapt.Engine;

namespace AstroAdapt.GraphQL
{
    public class ImageResponse
    {
        public ImageResponse(ImageData data)
        {
            Id = data.Id!.Value;
            Filename = data.FileName!;
            Data = Convert.ToBase64String(data.Image!);
        }

        public Guid Id { get; set; }

        public string Filename { get; set; }

        public string Data { get; set; }
    }
}
