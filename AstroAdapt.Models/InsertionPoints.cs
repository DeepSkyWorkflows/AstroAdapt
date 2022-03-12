namespace AstroAdapt.Models
{
    /// <summary>
    /// How a compoent can be inserted.
    /// </summary>
    public enum InsertionPoints
    {
        /// <summary>
        /// Must be flush to component at target.
        /// </summary>
        FlushToTarget,

        /// <summary>
        /// Prefers to be close to the target.
        /// </summary>
        PreferTarget,

        /// <summary>
        /// No preference.
        /// </summary>
        NoPreference,

        /// <summary>
        /// Prefers to be closer to sensor.
        /// </summary>
        PreferSensor,

        /// <summary>
        /// Must be flush to sensor.
        /// </summary>
        FlushToSensor,
    }
}
