namespace AstroAdapt.BlazorWasm.Workflow
{
    public interface IWorkflowAction<TWorkflowState>    
    {
        TWorkflowState Yield(TWorkflowState state);
        Action Yielded { get; set; }
    }
}
