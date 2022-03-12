 using System;
using System.Reflection.Metadata.Ecma335;

namespace AstroAdapt.Models
{
    /// <summary>
    /// Class used to solve backfocus on cameras.
    /// </summary>
    public class Solver
    {
        private readonly Inventory inventory;

        private Component Tail => Connections.Tail.Connection.TargetDirectionComponent!;

        private IEnumerable<(int level, Component component)> Components()
        {
            var current = Connections;
            var level = 0;
            while (current != null)
            {
                yield return (level++, current.Connection.TargetDirectionComponent!);
                current = current.SensorConnector;
            }
        }

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
            var connection = new Connection { TargetDirectionComponent = target };
            Connections = new Connector(connection);
            this.inventory = inventory.Clone();
        }

        /// <summary>
        /// Instantiates a new instance of the solver based on a parent.
        /// </summary>
        /// <param name="parent">The parent <see cref="Solver"/>.</param>
        /// <param name="sensorAdd">The component to add on the sensor side.</param>
        public Solver(Solver parent, Component sensorAdd)
        {
            inventory = parent.inventory.Clone();
            Target = parent.Target;
            Sensor = parent.Sensor;
            Connections = parent.Connections.Clone();
            Connections.Tail.ConnectTo(sensorAdd);
            inventory.Consume(sensorAdd);
            if (sensorAdd.Equals(Sensor))
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
        /// Gets the item that is relevant to compute back focus from
        /// </summary>
        public Component BackFocusItem =>
            Components()
            .Where(c => c.component.BackFocusMm > 0)
            .OrderByDescending(c => c.level)
            .First().component;

        /// <summary>
        /// Computes the current length from backfocus to sensor.
        /// </summary>
        /// <returns>The length to the sensor in millimeters.</returns>
        public double GetLengthToSensorMm()
        {
            var bf = BackFocusItem;
            var level = Components().First(c => c.component.Equals(bf)).level;
            return Components()
                .Where(c => c.level > level && !c.component.Equals(Sensor))
                .Sum(c => c.component.LengthMm - c.component.ThreadRecessMm);
        }

        /// <summary>
        /// Attempts to find a solution.
        /// </summary>
        public void Solve(Action<Solver> register)
        {
            if (Solved)
            {
                return;
            }

            var done = false;

            do
            {
                // always check for end connection first
                var (isCompatible, _) = Tail.IsCompatibleWith(Sensor);
                if (isCompatible)
                {
                    register(new Solver(this, Sensor));
                }

                var options = inventory.AvailableFor(Tail).ToArray();

                if (options.Length < 1)
                {
                    done = true;
                    continue;
                }

                if (options.Length > 1)
                {
                    foreach (var option in options[1..])
                    {
                        register(new Solver(this, option));
                    }
                }

                Connections.Tail.ConnectTo(options[0]);
                inventory.Consume(options[0]);
            }
            while (!done);
        }
    }
}
