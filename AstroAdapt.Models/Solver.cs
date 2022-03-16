namespace AstroAdapt.Models
{
    /// <summary>
    /// Class used to solve backfocus on cameras.
    /// </summary>
    public class Solver : ISolverInfo
    {
        private readonly Inventory inventory;

        private Component Tail => Connections.Tail.Connection.SensorDirectionComponent ??
            Connections.Tail.Connection.TargetDirectionComponent!;

        /// <summary>
        /// Instantiates a new instance of the solver.
        /// </summary>
        /// <param name="target">The component facing the target, usually a telescope.</param>
        /// <param name="sensor">The component receiving the image.</param>
        /// <param name="inventory">The inventory.</param>
        public Solver(Component target, Component sensor, Inventory inventory)
        {
            Target = target;
            Sensor = sensor;
            this.inventory = inventory.Clone();
            var connection = new Connection { TargetDirectionComponent = target };
            Connections = new Connector(connection);
        }

        /// <summary>
        /// Instantiates a new instance of the solver based on a parent.
        /// </summary>
        /// <param name="parent">The parent <see cref="Solver"/>.</param>
        /// <param name="sensorAdd">The component to add on the sensor side.</param>
        public Solver(Solver parent, Component sensorAdd)
        {
            inventory = parent.inventory.Clone(sensorAdd);
            Target = parent.Target;
            Sensor = parent.Sensor;
            Connections = parent.Connections.Clone();
            Connections.Tail.ConnectTo(sensorAdd);
            if (sensorAdd.SensorDirectionConnectionType == ConnectionTypes.Terminator)
            {
                Solved = true;
            }
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        public Component Target { get; private set; }

        /// <summary>
        /// Gets the sensor.
        /// </summary>
        public Component Sensor { get; private set; }

        /// <summary>
        /// Root for spanning connections.
        /// </summary>
        public Connector Connections { get; private set; }

        /// <summary>
        /// Gets where or not the connections were resolved.
        /// </summary>
        public bool Solved { get; private set; } = false;

        /// <summary>
        /// Gets the number of components in the solution.
        /// </summary>
        public int SolutionSize => Connections.GetComponents().Count();

        /// <summary>
        /// Unique signature of the solution.
        /// </summary>
        public byte[] Signature => Connections.GetComponents().OrderBy(c => c.level)
            .SelectMany(c => c.component.Signature).ToArray();

        /// <summary>
        /// Attempts to find a solution.
        /// </summary>
        /// <param name="fork">Action to fork the solution.</param>
        /// <param name="solved">Action to send a solution.</param>
        /// <param name="fractionBFDeviance">Fraction of back focus to allow for error. 0 for everything.</param>
        public void Solve(Action<Solver[]> fork, Action<Solution> solved, double fractionBFDeviance = 0.05)
        {
            if (Solved)
            {
                var solution = new Solution
                {
                    Target = Target,
                    Sensor = Sensor,
                    Connections = Connections.GetComponents().Select(c => c.component).ToList(),
                    BackFocusMm = Connections.SystemBackfocusMm,
                    LengthMm = Connections.LengthMm + Target.ThreadRecessMm + Sensor.ThreadRecessMm,
                };
                solved(solution);
                return;
            }

            var children = new List<Solver>();

            // always check for end connection first
            if (Tail.IsCompatibleWith(Sensor).isCompatible)
            {
                children.Add(new Solver(this, Sensor));
            }

            if (fractionBFDeviance > 0)
            {
                var tolerance = Connections.SystemBackfocusMm * 0.05;
                var length = Connections.LengthMm + Target.ThreadRecessMm + Sensor.ThreadRecessMm;
                var deviance = Connections.LengthMm - Connections.SystemBackfocusMm;

                if (deviance > tolerance)
                {
                    return;
                }
            }

            var options = Tail.GetCompatibleComponents(inventory.Available)
                .OrderByDescending(c => c.LengthMm);

            foreach (var option in options)
            {
                if (Tail.IsCompatibleWith(option).isCompatible)
                {
                    children.Add(new Solver(this, option));
                }

                if (option.IsReversible)
                {
                    var reversed = option.Clone();
                    reversed.Reverse();
                    if (Tail.IsCompatibleWith(reversed).isCompatible)
                    {
                        children.Add(new Solver(this, reversed));
                    }
                }
            }

            if (children.Count > 0)
            {
                fork(children.ToArray());
            }
        }
    }
}
