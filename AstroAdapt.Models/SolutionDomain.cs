using System.Collections;
using System.Collections.Concurrent;

namespace AstroAdapt.Models
{
    /// <summary>
    /// Solution for problems.
    /// </summary>
    public class SolutionDomain
    {
        private static readonly object solutionSync = new();

        private readonly int workerCount;
        private const byte MASK = 0xFF;
        private readonly Dictionary<Guid, Component> components = new();
        private readonly Dictionary<Guid, byte> bitMap = new();
        private readonly Dictionary<Guid, int> byteMap = new();
        private readonly Dictionary<Guid, byte[]> dependencies = new();
        
        /// <summary>
        /// Gets or sets the available domain of solutions.
        /// </summary>
        private HashSet<Solution> Solutions { get; set; } = new HashSet<Solution>();

        /// <summary>
        /// Queue to process work.
        /// </summary>
        private readonly ConcurrentQueue<SolverJob> SolverQueue = new();

        private Component? Target { get; set; }

        private Component? Sensor { get; set; }

        private double BackFocusTolerance { get; set; }
        /// <summary>
        /// Instantiates a new instance of the solution domain.
        /// </summary>
        /// <param name="workers">The number of workers to use.</param>
        public SolutionDomain(int workers = 10) => workerCount = workers;

        /// <summary>
        /// Gets the solutions.
        /// </summary>
        /// <returns>The solution set.</returns>
        public IEnumerable<Solution> GetSolutions() =>
            Solutions.OrderByDescending(s => s.Weight);

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

        /// <summary>
        /// Solve the connections.
        /// </summary>
        /// <param name="inventory">Available adapters for solutions.</param>
        /// <param name="target">The target to solve.</param>
        /// <param name="sensor">The sensor to solve.</param>
        /// <param name="backFocusTolerance">Percentage backfocus can be off.</param>
        public async Task SolveAsync(
            IEnumerable<Component> inventory,
            Component target,
            Component sensor,
            double backFocusTolerance = 0.05)
        {
            if (Solving)
            {
                throw new InvalidOperationException("Can't start a new solution before the old one finishes.");
            }

            components.Clear();
            bitMap.Clear();
            byteMap.Clear();
            dependencies.Clear();

            BackFocusTolerance = backFocusTolerance;
            Target = target;
            Sensor = sensor;

            if (inventory == null
                || !inventory.Any()
                || target == null
                || sensor == null)
            {
                throw new InvalidOperationException("Nothing to do!");
            }

            Solving = true;

            Solutions.Clear();
            SolverQueue.Clear();

            components.Add(target.Id, target);
            foreach (var component in inventory)
            {
                components.Add(component.Id, component);
            }
            components.Add(sensor.Id, sensor);
            var flags = Init(components.Values);
            BuildDependencies(components.Values);

            var imageTrain = new[]
            {
                (target.Id, false)
            };
            flags[0] &= 0b11111110; // consume target

            var result = Solve(flags, imageTrain);
            SolutionChanged?.Invoke(
                this,
                new SolutionEventArgs(
                    this,
                    SolutionEventTypes.SolverDone,
                    result));

            if (workerCount < 1)
            {
                Worker();
            }
            else
            {
                var workers = new Task[workerCount];

                for (int i = 0; i < workerCount; i++)
                {
                    Task task = new Task(Worker);
                    workers[i] = task;
                    task.Start();
                }
                await Task.WhenAll(workers);
            }

            Solving = false;
            SolutionChanged?.Invoke(this, new SolutionEventArgs(this, Solutions));
        }

        /// <summary>
        /// Initialize the bit-mapping for computing permutations.
        /// </summary>
        /// <param name="components">The list of components to work with.</param>
        /// <returns>The initial set of bits representing availability.</returns>
        /// <remarks>Simple bit map is true if the component is available, false if it's been used.</remarks>
        private byte[] Init(IEnumerable<Component> components)
        {
            var totalBytes = ((components.Count() - 1) >> 3) + 1;
            var flags = new byte[totalBytes];
            byte bitPos = 1;
            var bytePos = 0;
            byte currentByte = 0;
            foreach (var component in components)
            {
                bitMap.Add(component.Id, bitPos);
                byteMap.Add(component.Id, bytePos);
                currentByte |= bitPos;
                bitPos <<= 0x01;
                if (bitPos == 0)
                {
                    flags[bytePos] = currentByte;
                    currentByte = 0;
                    bitPos = 0x01;
                    bytePos++;
                }
            }
            if (currentByte > 0)
            {
                flags[bytePos] = currentByte;
            }

            return flags;
        }

        /// <summary>
        /// Gets the dependency signature for the component.
        /// </summary>
        /// <param name="components">The list of available components.</param>
        /// <param name="component">The component to test.</param>
        /// <param name="reversed">The call is for a reversed parent.</param>
        /// <returns>The dependencies.</returns>
        /// <remarks>Simple bit map shows true for compatible items and false for incompatible.</remarks>
        private byte[] GetDependencies(
            IEnumerable<Component> components,
            Component component,
            bool reversed = false)
        {
            var totalBytes = ((components.Count() - 1) >> 3) + 1;
            var deps = new byte[totalBytes];
            var sensor = component.Equals(Sensor);

            // to short circuit some solutions, we figure out which components can connect to the
            // sensor. If they are all in use, then there is no path to success. So this checks
            // "what components connect to sensor" vs other itemes which check "what components
            // can this one connect to"
            var options = sensor ?
               components.Skip(1) : component.GetCompatibleComponents(components);

            foreach (var option in options)
            {
                if (option.Equals(component))
                {
                    continue;
                }

                if (sensor)
                {
                    if (option.IsCompatibleWith(Sensor!).isCompatible ||
                        (option.IsReversible && option.Clone().Reverse()
                        .IsCompatibleWith(Sensor!).isCompatible))
                    {

                        deps[byteMap[option.Id]] |= bitMap[option.Id];
                    }
                }
                else
                {
                    if (component.IsCompatibleWith(option).isCompatible ||
                        (option.IsReversible &&
                        component.IsCompatibleWith(option.Clone().Reverse()).isCompatible))
                    {
                        deps[byteMap[option.Id]] |= bitMap[option.Id];
                    }
                }
            }

            if (!reversed && component.IsReversible)
            {
                var reversedDeps = GetDependencies(components, component.Clone().Reverse(), true);
                for (var idx = 0; idx < deps.Length; idx++)
                {
                    deps[idx] |= reversedDeps[idx];
                }
            }

            return deps;
        }

        /// <summary>
        /// Creates a dependency map (1 level deep) for each component.
        /// </summary>
        /// <param name="components">The list of components.</param>
        private void BuildDependencies(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                var deps = GetDependencies(components, component);
                dependencies.Add(component.Id, deps);
            }
        }

        /// <summary>
        /// Attempts to find a solution.
        /// </summary>
        /// <param name="flags">Current state of available components.</param>
        /// <param name="imageTrain">Current image train.</param>
        /// <returns>The reason for concluding the job.</returns>
        public SolverResults Solve(
            byte[] flags,
            (Guid id, bool reversed)[] imageTrain)
        {
            if (imageTrain[^1].id == Sensor!.Id) // made it to the end
            {
                var solution = new Solution
                {
                    Target = Target!,
                    Sensor = Sensor!,
                    BackFocusMm = Target!.BackFocusMm,
                    LengthMm = imageTrain.Skip(1).Sum(c => components[c.id].LengthMm)
                    + Target.ThreadRecessMm + Sensor.ThreadRecessMm,
                    Connections = imageTrain.Select(c => c.reversed ?
                    components[c.id].Clone().Reverse() :
                    components[c.id]).ToList(),
                };
                return Solved(solution); // might be rejected due to tolerance                
            }

            // this is the edge node we are considering
            (Guid rootId, bool reversed) = imageTrain[^1];
            var newComponent = reversed ? 
                components[rootId].Clone().Reverse() :
                components[rootId];

            // remove from inventory
            flags[byteMap[rootId]] &= (byte)(MASK ^ bitMap[rootId]);

            var flagBits = new BitArray(flags);

            var sensorDependencies = new BitArray(dependencies.Values.Last());

            // does a sensor solution still exist?
            if (!sensorDependencies.And(flagBits).Cast<bool>().Contains(true))
            {
                return SolverResults.NoSensorConnection;
            }

            var deps = new BitArray(dependencies[rootId].ToArray());

            // does any solution still exist?
            if (!deps.And(flagBits).Cast<bool>().Contains(true))
            {
                return SolverResults.DeadEnd;
            }

            // always check for end connection first
            if (newComponent.IsCompatibleWith(Sensor).isCompatible)
            {
                Spawn(new SolverJob(
                    flags.ToArray(), imageTrain.Union(new[] { (Sensor.Id, false) }).ToArray()));
            }

            if (BackFocusTolerance != 0 && imageTrain.Length > 0)
            {
                var tolerance = Target!.BackFocusMm * BackFocusTolerance;
                var length = imageTrain.Skip(1).Sum(c => components[c.id].LengthMm)
                    + Target.ThreadRecessMm + Sensor.ThreadRecessMm;

                var deviance = length - Target.BackFocusMm;

                if (deviance > tolerance)
                {
                    return SolverResults.OutsideTolerance;
                }
            }

            var options = new List<Component>();
            var depResolver = new BitArray(deps);
            var cidx = 0;
            var keys = components.Keys.ToList();

            // this will iterate each component and use a mask to determine what's available
            // for example 0110 means components 2 & 3 are available. The dependency might
            // be 1010 meaning it requires components 2 & 4. So the operation reveals that
            // 0b0110 & 0b1010 = 0b0010 so only component 2 is available to connect.
            while (cidx < components.Count)
            {
                if (depResolver[0])
                {
                    var option = components[keys[cidx]];
                    if (newComponent.IsCompatibleWith(option).isCompatible)
                    {
                        Spawn(new SolverJob(
                            flags.ToArray(),
                            imageTrain.Union(new[] { (option.Id, false) }).ToArray()));
                    }

                    // some components can be flipped and work just fine
                    if (option.IsReversible)
                    {
                        var reversedOption = option.Clone().Reverse();
                        if (newComponent.IsCompatibleWith(reversedOption).isCompatible)
                        {
                            Spawn(new SolverJob(
                            flags.ToArray(),
                            imageTrain.Union(new[] { (option.Id, true) }).ToArray()));
                        }
                    }
                }
                cidx++;
                depResolver = depResolver.RightShift(1);
            }

            return SolverResults.Forked;
        }

        /// <summary>
        /// Spawn a new job.
        /// </summary>
        /// <param name="job">The job.</param>
        private void Spawn(SolverJob job)
        {
            SolverQueue.Enqueue(job);
            SolutionChanged?.Invoke(
                this,
                new SolutionEventArgs(
                    this,
                    SolutionEventTypes.SolverSpawned));
        }

        /// <summary>
        /// Worker job to handle parallel processing.
        /// </summary>
        private void Worker()
        {
            var workToDo = !SolverQueue.IsEmpty;

            while (workToDo)
            {
                if (SolverQueue.TryDequeue(out var job) && job != null)
                {
                    var result = Solve(job.flags, job.solution);
                    SolutionChanged?.Invoke(
                        this,
                        new SolutionEventArgs(
                            this,
                            SolutionEventTypes.SolverDone,
                            result));
                }
                workToDo = !SolverQueue.IsEmpty;
            }
        }

        /// <summary>
        /// Called when a solution is found.
        /// </summary>
        /// <param name="solution">The <see cref="Solution"/>.</param>
        /// <returns>The result.</returns>
        private SolverResults Solved(Solution solution)
        {
            var tolerance = solution.BackFocusMm * BackFocusTolerance;
            if (solution.Deviance <= tolerance || BackFocusTolerance == 0)
            {
                solution.Weight = ComputeWeight(solution);
                solution.Signature = ComputeSignature(solution);
                bool dup = false;
                Monitor.Enter(solutionSync);
                try
                {
                    var count = Solutions.Count;
                    Solutions.Add(solution);
                    dup = Solutions.Count == count;
                }
                finally
                {
                    Monitor.Exit(solutionSync);
                }
                return dup ? SolverResults.Duplicate : SolverResults.Solved;
            }
            return SolverResults.OutsideTolerance;
        }

        /// <summary>
        /// Creates a signature unique to the image train physical properties and
        /// independent of intangibles like model name.
        /// </summary>
        /// <param name="solution">The solution to build the signature for.</param>
        /// <returns>The signature.</returns>
        private static string ComputeSignature(Solution solution)
        {
            var signatures = new List<byte[]>();
            foreach (var component in solution.Connections)
            {
                static byte lo(int i) => (byte)(i & 0b11111111);
                static byte hi(int i) => lo(i >> 8);
                static int toInt(double d) => (int)Math.Floor(d * 1000);
                var signature = new byte[]
                {
                    lo(toInt(component.BackFocusMm)),
                    hi(toInt(component.BackFocusMm)),
                    lo(toInt(component.LengthMm)),
                    hi(toInt(component.LengthMm)),
                    lo(toInt(component.ThreadRecessMm)),
                    hi(toInt(component.ThreadRecessMm)),
                    lo((int)component.ComponentType),
                    lo((int)component.InsertionPoint),
                    lo((int)component.TargetDirectionConnectionType),
                    lo((int)component.TargetDirectionConnectionSize),
                    lo((int)component.SensorDirectionConnectionType),
                    lo((int)component.TargetDirectionConnectionSize),
                    component.IsReversible ? (byte)0b0 : (byte)0b1
                };
                signatures.Add(signature);
            }
            return Convert.ToBase64String(signatures.SelectMany(c => c).ToArray());
        }

        /// <summary>
        /// Computes a factor based on component preferences for sorting results.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <returns>The weight.</returns>
        private static int ComputeWeight(Solution solution)
        {
            var inverseDeviance = 1.0 - solution.DeviancePct;
            if (inverseDeviance > 1)
            {
                inverseDeviance = 1;
            }
            var weight = inverseDeviance < 0 ? 1 : 10000 * inverseDeviance;
            var distanceFromTarget = 0;
            var distanceToSensor = solution.Connections.Count - 1;
            for (var idx = 0; idx < solution.Connections.Count; idx++)
            {
                var component = solution.Connections[idx];
                switch (component.InsertionPoint)
                {
                    case InsertionPoints.FlushToTarget:
                        if (distanceFromTarget == 0
                            || (distanceFromTarget == 1
                            && solution.Connections[0].LengthMm <= 5))
                        {
                            weight += 1000;
                        }
                        else
                        {
                            weight += -50 * distanceFromTarget;
                        }
                        break;
                    case InsertionPoints.PreferTarget:
                        if (distanceFromTarget == 0
                            || (distanceFromTarget == 1
                            && solution.Connections[0].LengthMm <= 5))
                        {
                            weight += 100;
                        }
                        else
                        {
                            weight += -10 * distanceFromTarget;
                        }
                        break;
                    case InsertionPoints.PreferSensor:
                        if (distanceToSensor == 0
                            || (distanceToSensor == 1
                            && solution.Connections[^1].LengthMm <= 5))
                        {
                            weight += 100;
                        }
                        else
                        {
                            weight += -10 * distanceToSensor;
                        }
                        break;
                    case InsertionPoints.FlushToSensor:
                        if (distanceToSensor == 0
                            || (distanceToSensor == 1
                            && solution.Connections[^1].LengthMm <= 5))
                        {
                            weight += 1000;
                        }
                        else
                        {
                            weight += -50 * distanceToSensor;    
                        }
                        break;
                    default:
                        weight--;
                        break;
                }
                distanceFromTarget++;
                distanceToSensor--;
            }
            return (int)Math.Floor(weight * 100);
        }
    }
}
