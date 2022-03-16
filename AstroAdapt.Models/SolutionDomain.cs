using System.Collections.Concurrent;

namespace AstroAdapt.Models
{
    /// <summary>
    /// Solution for problems.
    /// </summary>
    public class SolutionDomain
    {
        private readonly int workCount;

        /// <summary>
        /// Gets or sets the available domain of solutions.
        /// </summary>
        private ConcurrentBag<Solution> Solutions { get; set; } = new ConcurrentBag<Solution>();

        /// <summary>
        /// Queue to process work.
        /// </summary>
        private ConcurrentQueue<Solver> SolverQueue { get; set; } = new ConcurrentQueue<Solver>();

        /// <summary>
        /// Instantiates a new instance of the solution domain.
        /// </summary>
        /// <param name="workers">The number of workers to use.</param>
        public SolutionDomain(int workers = 10) => workCount = workers;

        /// <summary>
        /// Gets the solutions.
        /// </summary>
        /// <returns>The solution set.</returns>
        public IEnumerable<Solution> GetSolutions() =>
            Solutions.OrderBy(s => s.Deviance).ThenBy(s => s.ComponentCount);        

        /// <summary>
        /// Raised when the solution status changes;
        /// </summary>
        public event EventHandler<SolutionEventArgs>? SolutionChanged;

        /// <summary>
        /// Gets or sets a value indicating whether solutions are being processed.
        /// </summary>
        public bool Solving { get; private set; } = false;

        /// <summary>
        /// Gets the total solutions in play.
        /// </summary>
        public int NumberSolutions => SolverQueue.Count;

        /// <summary>
        /// Gets the valid solutions.
        /// </summary>
        public int ValidSolutions => Solutions.Count;

        private Predicate<IEnumerable<Component>>? solutionFilter;

        private double backFocusTolerance;

        /// <summary>
        /// Solve the connections.
        /// </summary>
        /// <param name="inventory">Available adapters for solutions.</param>
        /// <param name="target">The target to solve.</param>
        /// <param name="sensor">The sensor to solve.</param>
        /// <param name="backFocusTolerance">Percentage backfocus can be off.</param>
        /// <param name="solutionFilter">Filter for solutions.</param>
        public async Task SolveAsync(
            IEnumerable<Component> inventory,
            Component target,
            Component sensor,
            double backFocusTolerance = 0.05,
            Predicate<IEnumerable<Component>>? solutionFilter = null) 
        {
            if (Solving)
            {
                throw new InvalidOperationException("Can't start a new solution before the old one finishes.");
            }

            this.solutionFilter = solutionFilter;
            this.backFocusTolerance = backFocusTolerance;

            if (inventory == null || !inventory.Any() || target == null || sensor == null)
            {
                return;
            }

            Solving = true;

            Solutions.Clear();

            var solver = new Solver(target, sensor, new Inventory(inventory));
            
            Ingest(new [] { solver });

            var workers = new Task[workCount];

            for (int i = 0; i < workCount; i++)
            {
                Task task = new Task(Worker);
                workers[i] = task;
                task.Start();
            }

            await Task.WhenAll(workers);

            Solving = false;
            SolutionChanged?.Invoke(this, new SolutionEventArgs(this, Solutions));
        }

        private void Worker()
        {
            var workToDo = !SolverQueue.IsEmpty;
            while (workToDo)
            {
                if (SolverQueue.TryDequeue(out Solver? solver))
                {
                    if (solver != null)
                    {
                        solver.Solve(Ingest, Solved, backFocusTolerance);
                        if (!solver.Solved)
                        {
                            SolutionChanged?.Invoke(
                                this,
                                new SolutionEventArgs(this, SolutionEventTypes.SolutionFailed));
                        }
                    }
                }
                workToDo = !SolverQueue.IsEmpty;
            }
        }

        private void Solved(Solution solution)
        {
            if (!Solutions.Any(s => s.Signature.SequenceEqual(solution.Signature)))
            {
                var tolerance = solution.BackFocusMm * backFocusTolerance;
                if (solution.Deviance <= tolerance)
                {
                    if (solutionFilter == null || solutionFilter(solution.Connections) == true)
                    Solutions.Add(solution);                    
                }
            }
            SolutionChanged?.Invoke(this, new SolutionEventArgs(this, solution));
        }

        /// <summary>
        /// Ingest a solution.
        /// </summary>
        /// <param name="solvers">The solvers to run.</param>
        private void Ingest(Solver[] solvers)
        {
            foreach(var solver in solvers)
            {
                SolverQueue.Enqueue(solver);
            }
        }
    }
}
