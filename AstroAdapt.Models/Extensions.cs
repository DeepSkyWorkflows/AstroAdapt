namespace AstroAdapt.Models
{
    /// <summary>
    /// Useful extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Checks for compatibility between connection type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="other">The type to check it against.</param>
        /// <returns>A value indicating whether the connection types are compatible.</returns>
        public static bool IsCompatibleWith(this ConnectionTypes type, ConnectionTypes other) =>
        type switch
        {
            ConnectionTypes.Terminator => false,
            ConnectionTypes.Inserter => other == ConnectionTypes.Dual ||
                other == ConnectionTypes.Receiver,
            ConnectionTypes.Receiver => other == ConnectionTypes.Dual ||
                other == ConnectionTypes.Inserter,
            ConnectionTypes.Dual => other != ConnectionTypes.Terminator,
            _ => false,
        };

        /// <summary>
        /// Converts a connection type to a visual representation.
        /// </summary>
        /// <param name="type">The type of connection.</param>
        /// <param name="targetDirection">A value indicating whether the proposed connections is towards the telescope (target) or eyepiece (sensor).</param>
        /// <returns></returns>
        public static string AsVisualString(this ConnectionTypes type, bool targetDirection) =>
            type switch
            {
                ConnectionTypes.Dual => "][",
                ConnectionTypes.Receiver => targetDirection ? "]" : "[",
                ConnectionTypes.Inserter => targetDirection ? "\\\\" : "//",
                ConnectionTypes.Terminator => targetDirection ? "*" : "X",
                _ => "?"
            };

        /// <summary>
        /// Checks for compatibility between two adapters.
        /// </summary>
        /// <param name="component">The component to check.</param>
        /// <param name="other">The component to check against.</param>
        /// <param name="targetSide">A value indicating whether the direction of the connection is towards the target or the sensor.</param>
        /// <returns>A value indicating whether the two are compatible and an array of reasons why not if they are not compatible.</returns>
        /// <exception cref="ArgumentNullException">Thrown for null argument.</exception>
        public static (bool isCompatible, string errorMessage) IsCompatibleWith(
            this Component component, Component other, bool targetSide = false)
        {
            if (component == null)
            {
                return (false, "Component cannot be null.");
            }

            if (other == null)
            {
                return (false, "Other component cannot be null");
            }

            var (type, size) = targetSide ? (component.TargetDirectionConnectionType, component.TargetDirectionConnectionSize) :
                (component.SensorDirectionConnectionType, component.SensorDirectionConnectionSize);

            var (otherType, otherSize) = targetSide ? (other.SensorDirectionConnectionType, other.SensorDirectionConnectionSize) :
                (other.TargetDirectionConnectionType, other.TargetDirectionConnectionSize);

            var dir = targetSide ? "target" : "sensor";

            if (!type.IsCompatibleWith(otherType))
            {
                return (false, $"{type} is not compatible with {otherType}.");
            }

            if (size == ConnectionSizes.Zero)
            {
                return (false, $"Adapter size is zero facing the {dir}.");
            }

            if (otherSize == ConnectionSizes.Zero)
            {
                return (false, $"Other adapter has zero size.");
            }

            if (size == otherSize)
            {
                return (true, string.Empty);
            }

            return (false, $"{size} is not compatible with {otherSize}.");
        }

        /// <summary>
        /// Creates a connection between components.
        /// </summary>
        /// <param name="src">The source <see cref="Component"/>.</param>
        /// <param name="tgt">The target <see cref="Component"/>.</param>
        /// <param name="toTarget">A value indicating whether the connection is directed towards the target (otherwise the sensor).</param>
        /// <returns>The <see cref="Connection"/>.</returns>
        public static Connection ConnectTo(this Component src, Component tgt, bool toTarget = false)
        {
            var (isCompatible, errorMessage) = src.IsCompatibleWith(tgt, toTarget);
            if (isCompatible)
            {
                return new Connection
                {
                    TargetDirectionComponent = toTarget ? tgt : src,
                    SensorDirectionComponent = toTarget ? src : tgt,
                };
            }

            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// Connect to next in line.
        /// </summary>
        /// <param name="connector">The existing connector.</param>
        /// <param name="target">The target to connect to.</param>
        /// <returns>The new connector.</returns>  
        public static Connector ConnectTo(this Connector connector, Component target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (connector.Connection == null)
            {
                throw new InvalidOperationException("A connection is required.");
            }

            if (connector.Connection.SensorDirectionComponent == null)
            {
                var (compatible, errorMsg) = connector.Connection.TargetDirectionComponent!
                    .IsCompatibleWith(target);
                if (compatible == false)
                {
                    throw new InvalidOperationException(errorMsg);
                }
                connector.Connection.SensorDirectionComponent = target;
                return connector;
            }

            if (connector.Connection.SensorDirectionComponent == null)
            {
                var newConnector = new Connector(
                    connector,
                    new Connection
                    {
                        TargetDirectionComponent = connector.Connection.TargetDirectionComponent,
                        SensorDirectionComponent = target,
                    });
                return newConnector;
            }

            var (isCompatible, errorMessage) = connector.Connection.SensorDirectionComponent.IsCompatibleWith(target, false);

            if (isCompatible == false)
            {
                throw new InvalidOperationException(errorMessage);
            }

            var connection = connector.Connection.SensorDirectionComponent
                    .ConnectTo(target);                
            connector.SensorConnector = new Connector(connector, connection);
           
            return connector.SensorConnector;
        }

        /// <summary>
        /// Provides a set of cloned inventory sets representing every combination of
        /// reversing components.
        /// </summary>
        /// <param name="src">The source list of components.</param>
        /// <returns>The inventory permutations.</returns>
        public static Inventory[] AsInventorySets(this IEnumerable<Component> src)
        {
            if (src == null)
            {
                return Array.Empty<Inventory>();
            }

            Component[] baseSet = src.Where(s => s is not null).ToArray();
            var results = new List<Inventory>
            {
                new Inventory(baseSet)
            };

            if (baseSet.Any(c => c.IsReversible))
            {
                var permutations = 0x01 << baseSet.Count(c => c.IsReversible);
                for (var idx = 1; idx < permutations; idx++)
                {
                    var targetSet = baseSet.Select(c => c.Clone()).ToArray();
                    var flags = idx;
                    var reversible = targetSet.Where(c => c.IsReversible).ToArray();
                    var flagIdx = 0;

                    while (flags > 0)
                    {
                        if ((flags & 0x01) == 0x01)
                        {
                            reversible[flagIdx].Reverse();
                        }
                        flags >>= 0x01;
                        flagIdx++;
                    }
                    results.Add(new Inventory(targetSet));
                }
            }

            return results.ToArray();
        }
    }
}
