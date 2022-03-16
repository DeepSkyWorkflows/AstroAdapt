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
        /// <returns>A value indicating whether the two are compatible and an array of reasons why not if they are not compatible.</returns>
        /// <exception cref="ArgumentNullException">Thrown for null argument.</exception>
        public static (bool isCompatible, string errorMessage) IsCompatibleWith(
            this Component component, Component other) 
        {
            if (component == null)
            {
                return (false, "Component cannot be null.");
            }

            if (other == null)
            {
                return (false, "Other component cannot be null");
            }

            var (type, size) =
                (component.SensorDirectionConnectionType, component.SensorDirectionConnectionSize);

            var (otherType, otherSize) = 
                (other.TargetDirectionConnectionType, other.TargetDirectionConnectionSize);

            if (type == ConnectionTypes.Terminator || otherType == ConnectionTypes.Terminator)
            {
                return (false, "Can't connect to terminator.");
            }

            if (!type.IsCompatibleWith(otherType))
            {
                return (false, $"{type} is not compatible with {otherType}.");
            }

            if (size == ConnectionSizes.Zero)
            {
                return (false, $"Adapter size is zero facing the sensor.");
            }

            if (otherSize == ConnectionSizes.Zero)
            {
                return (false, $"Other adapter has zero size.");
            }

            if (size.IsCompatibleWith(otherSize))
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
        /// <returns>The <see cref="Connection"/>.</returns>
        public static Connection ConnectTo(this Component src, Component tgt)
        {
            var (isCompatible, errorMessage) = src.IsCompatibleWith(tgt);
            if (isCompatible)
            {
                return new Connection
                {
                    TargetDirectionComponent = src,
                    SensorDirectionComponent = tgt,
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

            var (isCompatible, errorMessage) =
                connector.Connection.SensorDirectionComponent.IsCompatibleWith(target);

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
        /// Gets compatible components for a target.
        /// </summary>
        /// <param name="src">The source <see cref="Component"/>.</param>
        /// <param name="available">The available connections.</param>
        /// <returns>The list of compatible adapters.</returns>
        public static IEnumerable<Component> GetCompatibleComponents(
            this Component src,
            IEnumerable<Component> available)
        {
            if (src.SensorDirectionConnectionSize == ConnectionSizes.Zero ||
                src.SensorDirectionConnectionType == ConnectionTypes.Terminator)
            {
                return Enumerable.Empty<Component>();
            }

            var query = available.AsQueryable();

            switch (src.SensorDirectionConnectionType)
            {
                case ConnectionTypes.Dual:
                    query = query.Where(c => c.TargetDirectionConnectionType != ConnectionTypes.Terminator
                    || (c.IsReversible && c.SensorDirectionConnectionType != ConnectionTypes.Terminator));
                    break;
                case ConnectionTypes.Receiver:
                    query = query.Where(c => c.TargetDirectionConnectionType == ConnectionTypes.Inserter
                    || (c.IsReversible && c.SensorDirectionConnectionType == ConnectionTypes.Inserter));
                    break;
                case ConnectionTypes.Inserter:
                    query = query.Where(c => c.TargetDirectionConnectionType == ConnectionTypes.Receiver
                    || (c.IsReversible && c.SensorDirectionConnectionType == ConnectionTypes.Receiver));
                    break;
                default:
                    break;
            }

            return query.Where(c => src.SensorDirectionConnectionSize.IsCompatibleWith(c.TargetDirectionConnectionSize)
            || (c.IsReversible && src.SensorDirectionConnectionSize.IsCompatibleWith(c.SensorDirectionConnectionSize)));
        }

        /// <summary>
        /// Determine size compatibility.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="otherSize">The size to check.</param>
        /// <returns>A value indicating whether the sizes are compatible.</returns>
        public static bool IsCompatibleWith(this ConnectionSizes size, ConnectionSizes otherSize)
        {
            if (size == ConnectionSizes.Zero || otherSize == ConnectionSizes.Zero)
            {
                return false;
            }

            return size switch
            {
                ConnectionSizes.M42With125Sleeve => otherSize == ConnectionSizes.M42With125Sleeve ||
                                        otherSize == ConnectionSizes.M42 ||
                                        otherSize == ConnectionSizes.OneQuarterInchSleeve,
                ConnectionSizes.OneQuarterInchSleeve => otherSize == ConnectionSizes.OneQuarterInchSleeve ||
otherSize == ConnectionSizes.M42With125Sleeve,
                ConnectionSizes.M48WithTwoInchSleeve => otherSize == ConnectionSizes.M48WithTwoInchSleeve ||
otherSize == ConnectionSizes.M48T ||
otherSize == ConnectionSizes.TwoInchSleeve,
                ConnectionSizes.TwoInchSleeve => otherSize == ConnectionSizes.TwoInchSleeve ||
otherSize == ConnectionSizes.M48WithTwoInchSleeve,
                _ => otherSize == size,
            };
        }
    }
}
