namespace AstroAdapt.Models
{
    /// <summary>
    /// Sizes available for connections
    /// </summary>
    public enum ConnectionSizes
    {
        /// <summary>
        /// Empty connection
        /// </summary>
        Zero,

        /// <summary>
        /// M12x0.5 "Webcam"
        /// </summary>
        M12,

        /// <summary>
        /// 1 inch "Video" thread
        /// </summary>
        Videox1in,

        /// <summary>
        /// 1.25" eyepiece, etc.
        /// </summary>
        M285x125,

        /// <summary>
        /// T/T2 thread
        /// </summary>
        M42,

        /// <summary>
        /// M42 with sleeve
        /// </summary>
        M42With125Sleeve,

        /// <summary>
        /// Maksutov thread
        /// </summary>
        M445,

        /// <summary>
        /// 48mm "wide" T, 2" filter
        /// </summary>
        M48T,

        /// <summary>
        /// M48 with 2" sleeve option
        /// </summary>
        M48WithTwoInchSleeve,

        /// <summary>
        /// 1" sleeve
        /// </summary>
        OneQuarterInchSleeve,

        /// <summary>
        /// 2" sleeve
        /// </summary>
        TwoInchSleeve,

        /// <summary>
        /// 2" SCT thread
        /// </summary>
        SmallSCT,

        /// <summary>
        /// 3.25" threads
        /// </summary>
        LargeSCT325,

        /// <summary>
        /// 3.28" threads
        /// </summary>
        LargeSCT328,
    }
}
