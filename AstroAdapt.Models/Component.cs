using System.Text;

namespace AstroAdapt.Models
{
    /// <summary>
    /// An actual component
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Gets or sets the unique id of the component.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the manufacturer of the component.
        /// </summary>
        public Manufacturer Manufacturer { get; set; } = new Manufacturer();

        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Model { get; set; } = "Generic";

        /// <summary>
        /// Gets or sets the distance the thread is recessed in millimeters. This can automatically
        /// reduce the amount of backfocus needed.
        /// </summary>
        public double ThreadRecessMm { get; set; } = 0;

        /// <summary>
        /// Gets or sets the back focus requirement. Backfocus is computed from the component that
        /// is closest to the sensor. 
        /// </summary>
        public double BackFocusMm { get; set; } = 0;

        /// <summary>
        /// Gets or sets the lenth of the component in millimeters.
        /// </summary>
        public double LengthMm { get; set; } = 0;

        /// <summary>
        /// Gets or sets the type of the component
        /// </summary>
        public ComponentTypes ComponentType { get; set; } = ComponentTypes.Other;

        /// <summary>
        /// Gets or sets the connection type facing the imaging target (sky).
        /// </summary>
        public ConnectionTypes TargetDirectionConnectionType { get; set; } = ConnectionTypes.Terminator;

        /// <summary>
        /// Gets or sets the size of the connection for cmpatbility.
        /// </summary>
        public ConnectionSizes TargetDirectionConnectionSize { get; set; } = ConnectionSizes.Zero;

        /// <summary>
        /// Gets or sets the connection type facing the imaging target (sky).
        /// </summary>
        public ConnectionTypes SensorDirectionConnectionType { get; set; } = ConnectionTypes.Terminator;

        /// <summary>
        /// Gets or sets the size of the connection for compatbility.
        /// </summary>
        public ConnectionSizes SensorDirectionConnectionSize { get; set; } = ConnectionSizes.Zero;

        /// <summary>
        /// Gets or sets a value indicating whether the directions of the component can be reversed.
        /// </summary>
        public bool IsReversible { get; set; } = false;

        /// <summary>
        /// Gets or sets the preferred insertion point.
        /// </summary>
        public InsertionPoints InsertionPoint { get; set; } = InsertionPoints.NoPreference;

        /// <summary>
        /// Code for report printing.
        /// </summary>
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>
        /// Reverse the orientation
        /// </summary>
        public Component Reverse()
        {
            if (IsReversible)
            {
                var (size, type) = (TargetDirectionConnectionSize, TargetDirectionConnectionType);
                (TargetDirectionConnectionSize, TargetDirectionConnectionType) =
                    (SensorDirectionConnectionSize, SensorDirectionConnectionType);
                (SensorDirectionConnectionSize, SensorDirectionConnectionType) = (size, type);
            }

            return this;
        }

        /// <summary>
        /// Creates a copy.
        /// </summary>
        /// <returns>The cloned copy.</returns>
        public Component Clone() => new()
        {
            Id = Id,
            ShortCode = ShortCode,            
            BackFocusMm = BackFocusMm,
            ComponentType = ComponentType,
            InsertionPoint = InsertionPoint,
            LengthMm = LengthMm,
            Manufacturer = Manufacturer,
            Model = Model,
            IsReversible = IsReversible,
            SensorDirectionConnectionSize = SensorDirectionConnectionSize,
            SensorDirectionConnectionType = SensorDirectionConnectionType,
            TargetDirectionConnectionSize = TargetDirectionConnectionSize,
            TargetDirectionConnectionType = TargetDirectionConnectionType,
            ThreadRecessMm = ThreadRecessMm,
        };                

        /// <summary>
        /// Equality.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>A value that is true when the other object is a <see cref="Component"/> with the same id.</returns>
        public override bool Equals(object? obj) => obj is Component other && Id == other.Id;

        /// <summary>
        /// Gets the hash code of the id.
        /// </summary>
        /// <returns>The hash code of the id.</returns>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Gets a string representation of the component.
        /// </summary>
        /// <returns>The string representation.</returns>
        public string ToShortString()
        {
            var sb = new StringBuilder();
            sb.Append($"{TargetDirectionConnectionType.AsVisualString(true)} ");
            sb.Append($"{ShortCode ?? Model}");
            sb.Append($"{SensorDirectionConnectionType.AsVisualString(false)} ");
            return sb.ToString();
        }

        /// <summary>
        /// Gets a string representation of the component.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{TargetDirectionConnectionType.AsVisualString(true)} {TargetDirectionConnectionSize} (");
            sb.Append(Manufacturer?.Name ?? string.Empty);
            sb.Append($" {Model} ");
            if (ThreadRecessMm > 0)
            {
                sb.Append($"thr={ThreadRecessMm}mm ");
            }

            if (BackFocusMm > 0)
            {
                sb.Append($"bf={BackFocusMm}mm ");
            }

            sb.Append($"len={LengthMm}mm)");
            sb.Append($"{SensorDirectionConnectionSize}{SensorDirectionConnectionType.AsVisualString(false)}");
            return sb.ToString();
        }
    }
}
