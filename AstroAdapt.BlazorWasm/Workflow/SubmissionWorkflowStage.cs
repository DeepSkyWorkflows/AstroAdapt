namespace AstroAdapt.BlazorWasm.Workflow
{
    public enum SubmissionWorkflowStage
    {
        RequestingCorrelationId,
        SubscribingToUpdates,
        SubscribingToFinalSolution,
        Solving,
        Done
    }
}
