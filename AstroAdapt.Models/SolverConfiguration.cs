namespace AstroAdapt.Models
{
    /// <summary>
    /// Configuration for a solution run.
    /// </summary>
    public class SolverConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="Component"/> list to use.
        /// </summary>
        public IEnumerable<Component>? Connections { get; set; }

        /// <summary>
        /// Gets or sets the target;
        /// </summary>
        public Component? Target { get; set; }

        /// <summary>
        /// Gets or sets the sensor.
        /// </summary>
        public Component? Sensor { get; set; }

        /// <summary>
        /// Gets or sets the backfocus tolerance.
        /// </summary>

        public double BackFocusTolerance { get; set; } = 0.05;

        /// <summary>
        /// Gets or sets the dela for UI.
        /// </summary>

        public bool DelayForUI { get; set; }

        /// <summary>
        /// Gets or sets the maximum solutions to allow.
        /// </summary>
        public int StopAfterNSolutions { get; set; }

        /// <summary>
        /// Gets or sets the maximum perfect solutions to allow.
        /// </summary>
        public int StopAfterNPerfectSolutions { get; set; }

        /// <summary>
        /// Gets or sets the maximum connections to allow.
        /// </summary>
        public int MaxConnectors { get; set; }

        /// <summary>
        /// Validate the properties.
        /// </summary>
        /// <param name="throwWhenInvalid">Whether or not to throw. </param>
        /// <returns></returns>
        public bool Validate(bool throwWhenInvalid = true)
        {
            var result = true;

            try
            {
                if (Connections == null || Connections.Count() < 2)
                {
                    throw new Exception($"{nameof(Connections)} should contain at least two components.");
                }

                if (Target == null)
                {
                    throw new Exception("Target cannot be null.");
                }

                if (Sensor == null)
                {
                    throw new Exception("Sensor cannot be null.");
                }

                if (BackFocusTolerance < 0)
                {
                    throw new Exception("Backfocus must be a positive number.");
                }

                if (StopAfterNPerfectSolutions < 0)
                {
                    throw new Exception("Can't stop after a negative number!");
                }

                if (StopAfterNSolutions < 0)
                {
                    throw new Exception("Can't stop after a negative number!");
                }

                if (MaxConnectors != 0 && MaxConnectors < 2)
                {
                    throw new Exception("Must have at least two connectors.");
                }

                if  (MaxConnectors >= (Connections.Count() -1 ))
                {
                    throw new Exception("Cap should be less than total connections");
                }
            }
            catch (Exception)
            {
                if (throwWhenInvalid)
                {
                    throw;
                }
                result = false;
            }

            return result;
        }
    }
}
