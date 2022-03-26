namespace AstroAdapt.BlazorWasm.Workflow
{
    /// <summary>
    /// Co-routine.
    /// </summary>
    public interface IWorkflowStep
    {
        /// <summary>
        /// Yield.
        /// </summary>
        void Yield();

        /// <summary>
        /// Has yielded.
        /// </summary>
        Action Yielded { get; set; }
    }
}
