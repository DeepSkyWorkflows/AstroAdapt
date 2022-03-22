using System.Collections.Concurrent;
using System.Runtime.Serialization;
using AstroAdapt.Engine;
using AstroAdapt.Models;
using HotChocolate.Subscriptions;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Handles long-running solutions.
    /// </summary>
    public class SolutionProcessingService
    {
        private long correlationId = 1;
        private readonly long OneSecond = TimeSpan.FromSeconds(1).Ticks;
        private readonly ObjectIDGenerator idGenerator = new ();
        private readonly IAstroApp astroService;
        private readonly ITopicEventSender eventSender;
        private readonly ConcurrentDictionary<long, long> timers = new ();
        private readonly ConcurrentDictionary<long, Func<Task<IEnumerable<Solution>>>> tasks = new ();
        private readonly object correlationMutex = new ();        

        /// <summary>
        /// Creates a new instance of the queue.
        /// </summary>
        /// <param name="astroApp">The astro app service to use.</param>
        /// <param name="eventSender">Sends events for subscriptions.</param>
        public SolutionProcessingService(
            IAstroApp astroApp,
            ITopicEventSender eventSender)
        {
            astroService = astroApp;
            this.eventSender = eventSender;
        }

        /// <summary>
        /// Queue the solution.
        /// </summary>
        /// <param name="targetId">The guid of the target.</param>
        /// <param name="sensorId">The guid of the sensor.</param>
        /// <param name="components">The available components.</param>
        /// <param name="backFocusTolerance">The backfocus tolerance.</param>
        /// <returns>A subscription correlation id.</returns>
        public long QueueSolution(
            Guid targetId,
            Guid sensorId,
            IEnumerable<Guid> components,
            double backFocusTolerance
            )
        {
            Monitor.Enter(correlationMutex);
            var id = correlationId++;
            Monitor.Exit(correlationMutex);
            timers.AddOrUpdate(id, DateTime.Now.Ticks, (o, n) => n);
            Task<IEnumerable<Solution>> task() =>
            astroService.SolveImageTrainAsync(
                        targetId,
                        sensorId,
                        components,
                        backFocusTolerance,
                        ProcessStats,
                        ProcessEventArgs,
                        correlationId: id);
            tasks.AddOrUpdate(id, task, (i, t) => t);
            return id;
        }

        /// <summary>
        /// Activates the queued task.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <returns>The task to wait for a soltuion.</returns>
        public async Task ActivateTaskAsync(long correlationId)
        {
            if (tasks.Remove(correlationId, out Func<Task<IEnumerable<Solution>>>? factory) &&
                factory != null)
            {
                var result = await factory();
                var finalSolution = new FinalSolution
                {
                    CorrelationId = correlationId,
                    Solutions = result.ToList(),
                };

                timers.Remove(correlationId, out long _);

                await eventSender.SendAsync(
                    $"{correlationId}_{nameof(FinalSolution)}",
                    finalSolution);
            }
        }

        private async void ProcessEventArgs(SolutionEventArgs obj)
        {
            var then = timers[obj.CorrelationId];
            if (DateTime.Now.Ticks - then < OneSecond)
            {
                return;
            }            

            timers[obj.CorrelationId] = DateTime.Now.Ticks;

            var update = new SolutionProcessingUpdate()
            {
                Args = obj,
                CorrelationId = obj.CorrelationId
            };

            await eventSender.SendAsync(
                $"{obj.CorrelationId}_{nameof(SolutionProcessingUpdate)}",
                update);
        }

        private async void ProcessStats(StatTracker obj)
        {
            var then = timers[obj.CorrelationId];
            if (DateTime.Now.Ticks - then < OneSecond)
            {
                return;
            }

            timers[obj.CorrelationId] = DateTime.Now.Ticks;

            var statistics = new Dictionary<SolverResults, long>();
            foreach(var sr in Enum.GetValues<SolverResults>())
            {
                statistics.Add(sr, obj[sr]);
            }
            var update = new SolutionProcessingUpdate()
            {
                Statistics = statistics,
                CorrelationId = obj.CorrelationId
            };

            await eventSender.SendAsync(
                $"{obj.CorrelationId}_{nameof(SolutionProcessingUpdate)}", update);
        }
    }
}
