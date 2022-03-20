namespace AstroAdapt.Engine
{
    /// <summary>
    /// Data for an image.
    /// </summary>
    public class ImageData
    {
        /// <summary>
        /// The id of the related component.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the bytes of the image.
        /// </summary>
        public byte[]? Image { get; set; }
    }
}
