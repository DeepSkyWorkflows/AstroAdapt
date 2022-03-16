namespace AstroAdapt.Models
{
    /// <summary>
    /// Enables traversal of solutions by linking connections.
    /// </summary>
    /// <remarks>
    /// A <see cref="Connection"/> is a link between two <seealso cref="Component"/> instances.
    /// A <see cref="Connector"/> hosts the connection along with references to other connections. 
    /// </remarks>
    public class Connector
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="Connector"/> class.
        /// </summary>
        /// <param name="connection">The <see cref="Connection"/> for this node.</param>
        public Connector(Connection connection) =>
            Connection = connection;

        /// <summary>
        /// Instantiates a new instance of the <see cref="Connector"/> class.
        /// </summary>
        /// <param name="target">The target-side existing connection.</param>
        /// <param name="connection">The <see cref="Connection"/> for this node.</param>
        public Connector(Connector target, Connection connection)
        {
            if (target.SensorConnector != null)
            {
                throw new InvalidOperationException("Connector can only have one connection.");
            }

            TargetConnector = target;
            Connection = connection;
            target.SensorConnector = this;
        }

        /// <summary>
        /// Gets the current <see cref="Connector"/> and any downstream ones.
        /// </summary>
        /// <returns>The iterable list of connectors.</returns>
        public IEnumerable<Connector> Downstream()
        {
            yield return this;

            if (SensorConnector == null)
            {
                yield break;
            }

            foreach (var downstreamConnector in SensorConnector.Downstream())
            {
                yield return downstreamConnector;
            }
        }

        /// <summary>
        /// Gets the back focus of the system.
        /// </summary>
        public double SystemBackfocusMm => GetComponents()
            .Where(c => c.component.BackFocusMm > 0)
            .OrderBy(c => c.level)
            .Select(c => c.component.BackFocusMm)
            .FirstOrDefault();            

        /// <summary>
        /// Gets the length of the image train from backfocus
        /// to sensor.
        /// </summary>
        public double LengthMm
        {
            get
            {
                var bfLevel = GetComponents()
                    .Where(c => c.component.BackFocusMm > 0)
                    .OrderBy(c => c.level)
                    .Select(c => c.component.BackFocusMm)
                                        .FirstOrDefault();
                var length = GetComponents()
                    .Where(c => c.level < bfLevel)
                    .Sum(c => c.component.LengthMm);
                               
                return length;
            }
        }

        /// <summary>
        /// Gets components for the <see cref="Connector"/>.
        /// </summary>
        /// <returns>The list of components.</returns>
        public IEnumerable<(int level, Component component)> GetComponents()
        {
            var downstream = new List<(int level, Component val)>();
            if (SensorConnector != null)
            {
                downstream.AddRange(SensorConnector.GetComponents());
            }
            var level = downstream.Any() ? downstream.Max(d => d.level) + 1 : 1;

            if (TargetConnector == null)
            {
                if (Connection.TargetDirectionComponent != null)
                {
                    yield return (level + 1, Connection.TargetDirectionComponent);
                }
            }
            if (Connection.SensorDirectionComponent != null)
            {
                yield return (level, Connection.SensorDirectionComponent);
            }
            foreach (var ds in downstream.OrderByDescending(ds => ds.level))
            {
                yield return ds;
            }
        }

        /// <summary>
        /// Gets the backfocus of the connection.
        /// </summary>
        public double BackFocusMm
        {
            get
            {
                if (SensorConnector != null && SensorConnector.BackFocusMm > 0 )
                {
                    return SensorConnector.BackFocusMm;
                }

                return Connection.BackFocusMm;
            }
        }

        /// <summary>
        /// Gets or sets the upstream <see cref="Connector"/>
        /// </summary>
        public Connector? TargetConnector { get; set; }

        /// <summary>
        /// The tail of the connection train.
        /// </summary>
        public Connector Tail => SensorConnector == null ?
            this : SensorConnector.Tail;

        /// <summary>
        /// Gets the <see cref="Connection"/> for this node.
        /// </summary>
        public Connection Connection { get; private set; }

        /// <summary>
        /// Gets the <see cref="Connector"/> in the sensor direction.
        /// </summary>
        public Connector? SensorConnector { get; set; }

        /// <summary>
        /// Clones the connector.
        /// </summary>
        /// <returns>The cloned copy.</returns>
        public Connector Clone()
        {
            var connector = new Connector(Connection.CloneIfMutable())
            {
                TargetConnector = TargetConnector
            };

            if (SensorConnector != null)
            {
                connector.SensorConnector = SensorConnector.Clone();
            }

            return connector;
        }

        /// <summary>
        /// Get the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var up = TargetConnector == null ? "*" : TargetConnector.Connection.ToString();
            var down = SensorConnector == null ? "X" : SensorConnector.Connection.ToString();
            return $"{up} <==> {Connection} <==> {down}";
        } 
    } 
}
