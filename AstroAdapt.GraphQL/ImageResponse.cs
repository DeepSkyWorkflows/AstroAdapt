using AstroAdapt.Engine;
using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// A set of image data.
    /// </summary>
    public class ImageResponse
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="data">The data to process.</param>
        public ImageResponse(ImageData data)
        {
            Id = data.Id!.Value;
            Filename = data.FileName!;
            Type = data.Type!;
            Data = Convert.ToBase64String(data.Image!);
        }

        /// <summary>
        /// The id of the component the image correlates to.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of  the component.
        /// </summary>
        public ComponentTypes Type { get; set; }

        /// <summary>
        /// The filename for browser downloads and to determine the file type.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The base64 encoded image bytes.
        /// </summary>
        public string Data { get; set; }
    }
}
