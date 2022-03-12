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
        /// Adds space to the image train
        /// </summary>
        Spacer,

        /// <summary>
        /// Manages filteres
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
        /// Other component type
        /// </summary>
        Other,
    }
}
