namespace AstroAdapt.Models
{
    /// <summary>
    /// Helper for setting up configuration.
    /// </summary>
    public class SolverConfigurationBuilder
    {
        /// <summary>
        /// The configuration object.
        /// </summary>
        private SolverConfiguration configuration;

        /// <summary>
        /// Get the config insance.
        /// </summary>
        public SolverConfigurationBuilder() => configuration = new SolverConfiguration();

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public SolverConfiguration Configuration
        {
            get
            {
                configuration.Validate(true);
                return configuration;
            }
        }

        /// <summary>
        /// Add connections to the solution.
        /// </summary>
        /// <param name="connections">The connections to add.</param>
        /// <returns>The builder to chain configuration.</returns>
        public SolverConfigurationBuilder UsingConnections(IEnumerable<Component> connections)
        {
            if (connections == null || connections.Count() < 2)
            {
                throw new Exception($"{nameof(connections)} should contain at least two components.");
            }

            configuration.Connections = connections;
            return this;
        }

        /// <summary>
        /// Add target to the solution.
        /// </summary>
        /// <returns>The builder to chain configuration.</returns>
        public SolverConfigurationBuilder FromTarget(Component target)
        {
            if (target == null)
            {
                throw new Exception("Target cannot be null.");
            }
            configuration.Target = target;
            return this;
        }

        /// <summary>
        /// Add sensor to the solution.
        /// </summary>
        /// <returns>The builder to chain configuration.</returns>
        public SolverConfigurationBuilder ToSensor(Component sensor)
        {
            if (sensor == null)
            {
                throw new Exception("Sensor cannot be null.");
            }
            configuration.Sensor = sensor;
            return this;
        }

        /// <summary>
        /// Add back focus tolerance to the solution.
        /// </summary>
        /// <returns>The builder to chain configuration.</returns>
        public SolverConfigurationBuilder WithBackfocusToleranceOf(double bft)
        {
            if (bft < 0)
            {
                throw new Exception("Backfocus must be a positive number.");
            }

            configuration.BackFocusTolerance = bft;
            return this;
        }

        /// <summary>
        /// Adds a delay so systems like Blazor Wasm can refresh the UI.
        /// </summary>
        /// <returns>Itself.</returns>
        public SolverConfigurationBuilder AddDelayForUi()
        {
            configuration.DelayForUI = true;
            return this;
        }

        /// <summary>
        /// Limits total solutions.
        /// </summary>
        /// <param name="stopAfter">The solutions limit.</param>
        /// <returns>The builder.</returns>
        public SolverConfigurationBuilder StopWhenSolutionsCountIs(int stopAfter)
        {
            if (stopAfter < 0)
            {
                throw new Exception("Can't stop after a negative number!");
            }

            configuration.StopAfterNSolutions = stopAfter;
            return this;
        }

        /// <summary>
        /// Limits total perfect solutions.
        /// </summary>
        /// <param name="stopAfter">The perfect solutions limit.</param>
        /// <returns>The builder.</returns>
        public SolverConfigurationBuilder StopWhenPerfectSolutionsCountIs(int stopAfter)
        {
            if (stopAfter < 0)
            {
                throw new Exception("Can't stop after a negative number!");
            }

            configuration.StopAfterNPerfectSolutions = stopAfter;
            return this;
        }

        /// <summary>
        /// Limits total connectors in a solution.
        /// </summary>
        /// <param name="stopAfter">The connectors limit.</param>
        /// <returns>The builder.</returns>
        public SolverConfigurationBuilder LimitConnectionsTo(int stopAfter)
        {
            if (stopAfter != 0 && stopAfter < 2)
            {
                throw new Exception("Must have at least 2 connectors.");
            }

            configuration.MaxConnectors = stopAfter;
            return this;
        }
    }
}
