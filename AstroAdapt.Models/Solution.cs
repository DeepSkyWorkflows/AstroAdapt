namespace AstroAdapt.Models
{
    /// <summary>
    /// A solved result.
    /// </summary>
    public class Solution
    {
        /// <summary>
        /// Creates an instance of the solut ion.
        /// </summary>
        public Solution()
        { }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public Component Target { get; set; } = null!;

        /// <summary>
        /// Gets or sets the sensor.
        /// </summary>
        public Component Sensor { get; set; } = null!;

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public List<Component> Connections { get; set; } = new List<Component>();

        /// <summary>
        /// Gets the count of components.
        /// </summary>
        public int ComponentCount => Connections.Count;

        /// <summary>
        /// Gets or sets the backfocus of the system.
        /// </summary>
        public double BackFocusMm { get; set; }

        /// <summary>
        /// Gets or sets the length of the system.
        /// </summary>
        public double LengthMm { get; set; }

        /// <summary>
        /// Weight for sorting purposes.
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets the deviance from backfocus of the system.
        /// </summary>
        public double Deviance => Math.Abs(BackFocusMm - LengthMm);

        /// <summary>
        /// Percentage of deviance.
        /// </summary>
        public double DeviancePct => Deviance / BackFocusMm;

        /// <summary>
        /// Equality of solutions.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>A value indicating whether they are equal.</returns>
        public override bool Equals(object? obj) => obj is Solution solution &&
            Signature == solution.Signature;

        /// <summary>
        /// Gets the hash code. 
        /// </summary>
        /// <returns>The signature hash code.</returns>
        public override int GetHashCode() => Signature.GetHashCode();

        /// <summary>
        /// A unique signature for the solution.
        /// </summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public string ToShortString() =>
            $"Weight: {Weight} BF: {BackFocusMm} Len: {LengthMm} Deviance: {Deviance} (PCT: {DeviancePct * 100}%)"
            + $"{Environment.NewLine}"
            + $"{string.Join(" | ", Connections.Select(c => c.ToShortString()).ToArray())}";

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() =>
            $"Weight: {Weight} BF: {BackFocusMm} Len: {LengthMm} Deviance: {Deviance} (PCT: {DeviancePct * 100}%)"
            + $"{Environment.NewLine}"
            + $"{string.Join(" | ", Connections.Select(c => c.ToString()).ToArray())}";
    }
}
