namespace AstroAdapt.BlazorWasm.Workflow
{
    /// <summary>
    /// Asynchronous workflows.
    /// </summary>
    public class WorkflowEngine
    {
        /// <summary>
        /// Enumerator.
        /// </summary>
        private readonly IEnumerator<IWorkflowStep> enumerator;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        public WorkflowEngine(IEnumerable<IWorkflowStep> workflow) =>
            enumerator = workflow.GetEnumerator();

        /// <summary>
        /// Chain steps.
        /// </summary>
        private void Yielded()
        {
            if (!enumerator.MoveNext())
                return;

            var next = enumerator.Current;
            next.Yielded = Yielded;
            next.Yield();
        }

        /// <summary>
        /// Kick off a workflow.
        /// </summary>
        /// <param name="workflow">The workflow start.</param>
        public static void Begin(object workflow)
        {
            if (workflow is IWorkflowStep)
            {
                workflow = new[] { workflow as IWorkflowStep };
            }

            if (workflow is IEnumerable<IWorkflowStep> nestedCoroutine)
            {
                new WorkflowEngine(nestedCoroutine).Yielded();
            }
        }
    }
}
