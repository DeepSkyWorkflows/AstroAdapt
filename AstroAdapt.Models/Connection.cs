namespace AstroAdapt.Models
{
    /// <summary>
    /// Represents a connection between components.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Gets the unique identifier of the connection.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the next component in the direction of the target/sky.
        /// </summary>
        public Component? TargetDirectionComponent { get; set; } = null;

        /// <summary>
        /// Gets or sets the next component in the direction of the sensor or eyepiece.
        /// </summary>
        public Component? SensorDirectionComponent { get; set; } = null;

        /// <summary>
        /// Clone the connection.
        /// </summary>
        /// <returns>A new copy.</returns>
        public Connection Clone() => new()
        {
            SensorDirectionComponent = SensorDirectionComponent,
            TargetDirectionComponent = TargetDirectionComponent,
        };        

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"{TargetDirectionComponent} ... {SensorDirectionComponent}";

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hashcode of the id.</returns>
        public override int GetHashCode() => Id.GetHashCode();
    }
}
