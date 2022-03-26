namespace AstroAdapt.BlazorWasm.Workflow
{
    /// <summary>
    /// A workflow step.
    /// </summary>
    public class WorkflowAction : IWorkflowStep
    {
        /// <summary>
        /// A flag indicating whether to execute the step immediately.
        /// </summary>
        private readonly bool immediate = false;

        /// <summary>
        /// Action to execute.
        /// </summary>
        public Action Execute { get; set; } = () => { };

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public WorkflowAction()
        {

        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="immediate">Whether or not to execute immediately.</param>
        public WorkflowAction(bool immediate)
        {
            this.immediate = immediate;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>        
        public WorkflowAction(Action action)
        {
            immediate = false;
            Execute = action;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public WorkflowAction(Action action, bool immediate)
        {
            this.immediate = immediate;
            Execute = action;
        }

        /// <summary>
        /// Execute and chain.
        /// </summary>
        public void Yield()
        {
            Execute();
            if (immediate)
            {
                Yielded();
            }
        }

        /// <summary>
        /// Yielded.
        /// </summary>
        public Action Yielded { get; set; } = () => { };
    }
}
