namespace AstroAdapt.Models
{
    /// <summary>
    /// Types of components in the image train.
    /// </summary>
    public enum ComponentTypes
    {
        /// <summary>
        /// Optical tube assembly
        /// </summary>
        OTA,

        /// <summary>
        /// Adapt one fitting to another
        /// </summary>
        Adapter,

        /// <summary>
        /// Attaches to the scope
        /// </summary>
        VisualBack,

        /// <summary>
        /// Adds space to the image train
        /// </summary>
        Spacer,

        /// <summary>
        /// Spacer flush with sensor (usually).
        /// </summary>
        NosePiece,

        /// <summary>
        /// Manages filters
        /// </summary>
        FilterWheel,

        /// <summary>
        /// Holds a filter
        /// </summary>
        FilterDrawer,

        /// <summary>
        /// Direct filter
        /// </summary>
        Filter,

        /// <summary>
        /// Barlow lens
        /// </summary>
        Barlow,

        /// <summary>
        /// Diagonal for comfort viewing
        /// </summary>
        Diagonal,

        /// <summary>
        /// Focal reducer or field flattener
        /// </summary>
        FocalReducer,

        /// <summary>
        /// Off-axis guider
        /// </summary>
        OAG,

        /// <summary>
        /// Sensor such as camera (CCD, CMOS, etc.)
        /// </summary>
        Sensor,

        /// <summary>
        /// Eye piece for viewing
        /// </summary>
        Eyepiece,

        /// <summary>
        /// Allows rotation
        /// </summary>
        CompressionRing,

        /// <summary>
        /// Other component type
        /// </summary>
        Other,
    }
}
