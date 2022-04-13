namespace AstroAdapt.Models
{
    /// <summary>
    /// Holds info about the progress of a solution.
    /// </summary>
    public sealed class StatTracker : IDisposable
    {
        private bool disposedValue;
        private static readonly object mutex = new();
        private readonly SolutionDomain sd;
        private readonly IDictionary<SolverResults,long> statistics =
            new Dictionary<SolverResults, long>();
        private readonly Action<StatTracker> callback = s => { };

        /// <summary>
        /// Gets the last domain-level event.
        /// </summary>
        public SolutionEventTypes LastEvent { get; private set; } =
            SolutionEventTypes.SolverSpawned;

        /// <summary>
        /// Gets the available result types.
        /// </summary>
        public static IEnumerable<SolverResults> AvailableResultTypes =>
            Enum.GetValues<SolverResults>();

        /// <summary>
        /// Indexer to tracker.
        /// </summary>
        /// <param name="idx">The results.</param>
        /// <returns>The count for the result.</returns>
        public long this[SolverResults idx] =>
            statistics.ContainsKey(idx) ? statistics[idx] : 0;

        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        public long CorrelationId { get; set; }

        /// <summary>
        /// Gets the number of queued solutions.
        /// </summary>
        public long QueuedSolutions { get; set; }

        /// <summary>
        /// Creates a new instance and registers for updates.
        /// </summary>
        /// <param name="sd">The <see cref="SolutionDomain"/>.</param>
        /// <param name="callback">Optional callback on status changes.</param>
        public StatTracker(SolutionDomain sd, Action<StatTracker>? callback = null)
        {
            this.sd = sd;
            foreach (var type in AvailableResultTypes)
            {
                statistics.Add(type, 0);
            }
            if (callback != null)
            {
                this.callback = callback;
            }
            sd.SolutionChanged += Sd_SolutionChanged;
        }

        private void Sd_SolutionChanged(object? sender, SolutionEventArgs e)
        {
            LastEvent = e.EventType;
            Monitor.Enter(mutex);
            statistics[e.SolverResult]++;
            if (sender is SolutionDomain sd)
            {
                QueuedSolutions = sd.NumberSolutions;
            }
            Monitor.Exit(mutex);
            callback(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sd.SolutionChanged -= Sd_SolutionChanged;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
