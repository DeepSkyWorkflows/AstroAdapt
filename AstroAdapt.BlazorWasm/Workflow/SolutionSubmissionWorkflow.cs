using AstroAdapt.BlazorWasm.GraphQL;
using System.Reactive.Linq;

namespace AstroAdapt.BlazorWasm.Workflow
{
    public class SolutionSubmissionWorkflow : ISolutionSubmissionWorkflow
    {
        private readonly SolverWorkflow parentWorkflow;
        private readonly AstroClient client;
        private IDisposable? updateWatcher;
        private IDisposable? finalWatcher;
        private Action stateHasChanged;
        private Action? ready;

        public long? CorrelationId { get; set; }

        public IGetSolutionUpdatesResult? SolutionUpdate { get; private set; }

        public IGetFinalSolutionResult? SolutionResult { get; private set; }

        public void Ready()
        {
            if (ready is not null)
            {
                ready();
            }
        }

        public SolutionSubmissionWorkflow(
            SolverWorkflow workflow,
            AstroClient client,
            Action stateHasChanged)
        {
            parentWorkflow = workflow;
            this.client = client;
            this.stateHasChanged = stateHasChanged;
        }

        public SubmissionWorkflowStage Stage { get; private set; } = SubmissionWorkflowStage.RequestingCorrelationId;

        public IEnumerable<IWorkflowStep> WorkflowSteps()
        {
            var submit = new WorkflowAction(async () =>
            {
                var result = await client.SubmitProblem.ExecuteAsync(
                    parentWorkflow.Target!.Id,
                    parentWorkflow.Sensor!.Id,
                    parentWorkflow.SelectedInventory!.Select(si => si.Id).ToList(),
                    0.1);
                CorrelationId = result.Data!.SolutionSubscriptionId;
                ready!();
            }, false);

            ready = () => submit.Yielded();
            yield return submit;

            yield return new WorkflowAction(() =>
            {
                Stage = SubmissionWorkflowStage.SubscribingToUpdates;
                stateHasChanged();
                updateWatcher = client.GetSolutionUpdates
                    .Watch(CorrelationId!.ToString())
                    .Select(message => message.Data)
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .Subscribe(message => OnNextUpdate(message!));
            }, true);

            yield return new WorkflowAction(() =>
            {
                Stage = SubmissionWorkflowStage.SubscribingToFinalSolution;
                stateHasChanged();
                finalWatcher = client.GetFinalSolution
                    .Watch(CorrelationId!.ToString())
                    .Select(message => message.Data)
                    .Subscribe(message => OnFinal(message!));
            }, true);

            var solving = new WorkflowAction(() =>
            {
                Stage = SubmissionWorkflowStage.Solving;
                stateHasChanged();
            }, false);

            ready = () => solving.Yielded();
            yield return solving;

            yield return new WorkflowAction(() =>
            {
                Stage = SubmissionWorkflowStage.Done;
                stateHasChanged();
            }, true);
        }

        private void OnNextUpdate(IGetSolutionUpdatesResult getSolutionUpdatesResult)
        {
            SolutionUpdate = getSolutionUpdatesResult;
            stateHasChanged();
        }

        private void OnFinal(IGetFinalSolutionResult getFinalSolutionResult)
        {
            SolutionResult = getFinalSolutionResult;
            stateHasChanged();
            updateWatcher.Dispose();
            finalWatcher.Dispose();
            ready();
        }


    }
}
