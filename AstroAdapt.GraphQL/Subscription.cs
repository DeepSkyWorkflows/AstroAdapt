using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Subscriptions for GraphQL.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Update when the entire solution set is found.
        /// </summary>
        /// <remarks>
        /// This subscription is necessary to kick off the solution.
        /// </remarks>
        /// <param name="correlationId">The unique id for the job.</param>
        /// <param name="receiver">The event receiver.</param>
        /// <param name="service">The service to process the solution.</param>
        /// <returns>The requested payload.</returns>
        [SubscribeAndResolve]
        public ValueTask<ISourceStream<FinalSolution>> ProblemSolved(
            string correlationId,
            [Service] ITopicEventReceiver receiver,
            [Service] SolutionProcessingService service)
        {
            var topic = $"{correlationId}_{nameof(FinalSolution)}";
            var sub = receiver.SubscribeAsync<string, FinalSolution>(topic);
            Task.Run(() =>
            { 
                Thread.Sleep(500);
                service.ActivateTaskAsync(long.Parse(correlationId)).ConfigureAwait(false);
            });
            return sub;
        }

        /// <summary>
        /// Subscription for interim progress updates.
        /// </summary>
        /// <remarks>
        /// Updates are rate-limited to one every 200ms. The client will not receive all
        /// messages, so for a guaranteed solution set subscribe to "Problem Solved."
        /// </remarks>
        /// <param name="correlationId">The unique id.</param>
        /// <param name="receiver">The topic receiver.</param>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        [SubscribeAndResolve]
        public ValueTask<ISourceStream<SolutionProcessingUpdate>> SolutionUpdated(
            string correlationId,
            [Service] ITopicEventReceiver receiver,
            [Service]SolutionProcessingService service)
        {
            var topic = $"{correlationId}_{nameof(SolutionProcessingUpdate)}";
            var sub = receiver.SubscribeAsync<string, SolutionProcessingUpdate>(topic);
            return sub;
        }
    }
}
