namespace AstroAdapt.BlazorWasm.Workflow
{
    public abstract class BaseWorkflowAction<TWorkflowState>
        : IWorkflowAction<TWorkflowState>
    {
        protected abstract (bool continueWhenDone, TWorkflowState newState)
            DoWork(TWorkflowState state);

        public Action Yielded { get; set; } = () => { };

        public TWorkflowState Yield(TWorkflowState state)
        {
            var (continueWhenDone, newState) =
                DoWork(state);

            if (continueWhenDone)
            {
                Yielded();
            }

            return newState;
        }
    }
}
