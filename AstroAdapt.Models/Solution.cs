namespace AstroAdapt.Models
{
    /// <summary>
    /// A solved result.
    /// </summary>
    public class Solution
    {
        private byte[]? signature = null;

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
        /// Gets or sets the deviance from backfocus of the system.
        /// </summary>
        public double Deviance => Math.Abs(BackFocusMm - LengthMm);

        /// <summary>
        /// Gets the unique signature of the solution.
        /// </summary>
        public byte[] Signature
        {
            get
            {
                if (signature == null)
                {
                    signature = Connections.SelectMany(c => c.Signature).ToArray();
                }
                return signature;
            }
        }

        /// <summary>
        /// Equality of solutions.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>A value indicating whether they are equal.</returns>
        public override bool Equals(object? obj) => obj is Solution solution &&
            Signature.SequenceEqual(solution.Signature);

        /// <summary>
        /// Gets the hash code. 
        /// </summary>
        /// <returns>The signature hash code.</returns>
        public override int GetHashCode() => Signature.GetHashCode();

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() =>
            $"BF: {BackFocusMm} Len: {LengthMm} {string.Join(" | ", Connections.Select(c => c.ToString()).ToArray())}";
    }
}
