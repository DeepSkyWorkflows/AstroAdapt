using AstroAdapt.BlazorWasm.GraphQL;

namespace AstroAdapt.BlazorWasm.Workflow
{
    public interface ISolutionSubmissionWorkflow
    {
        long? CorrelationId { get; set; }
        IGetFinalSolutionResult? SolutionResult { get; }
        IGetSolutionUpdatesResult? SolutionUpdate { get; }
        SubmissionWorkflowStage Stage { get; }

        void Ready();
        IEnumerable<IWorkflowStep> WorkflowSteps();
    }
}