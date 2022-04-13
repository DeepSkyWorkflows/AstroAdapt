using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public class SolverStats
    {
        private readonly DateTime started;

        public string Title { get; private set; }

        public long TotalAttempts { get; private set; }

        public long ForkedSolutions { get; private set; }

        public long AbortedNoSensor { get; private set; }
        public long IgnoredAsDuplicate { get; private set; }

        public long AbortedDeadEnd { get; private set; }

        public long AbortedOutsideTolerance { get; private set; }
        public long Solved { get; private set; }
        public TimeSpan RunningTime => DateTime.Now - started;

        public Solution? LastSolution { get; set; }

        public long QueuedSolutions { get; private set; }

        public List<Solution> Solutions { get; private set; } = new List<Solution>();

        public SolverStats(Component target, Component sensor)
        {
            Title = $"Solving for backfocus of {target.BackFocusMm}mm from {target.Manufacturer?.Name} {target.Model} to {sensor.Manufacturer?.Name} {sensor.Model}";
            started = DateTime.Now;            
        }

        public void Refresh(StatTracker stats)
        {            
            Solved = stats[SolverResults.Solved];
            TotalAttempts = stats[SolverResults.Info];
            ForkedSolutions = stats[SolverResults.Forked];
            AbortedNoSensor = stats[SolverResults.NoSensorConnection];
            IgnoredAsDuplicate = stats[SolverResults.Duplicate];
            AbortedDeadEnd = stats[SolverResults.DeadEnd];
            AbortedOutsideTolerance = stats[SolverResults.OutsideTolerance];
            QueuedSolutions = stats.QueuedSolutions;
        }

    }
}
