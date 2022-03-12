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
            var connector = new Connector(Connection.Clone());
            if (TargetConnector != null)
            {
                connector.TargetConnector = TargetConnector.Clone();
            }
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
