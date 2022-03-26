using AstroAdapt.BlazorWasm.GraphQL;

namespace AstroAdapt.BlazorWasm.Workflow
{
    public class SolverWorkflow
    {
        private readonly Action stateHasChanged = () => { };
        private readonly AstroClient client;
        private Action? ready = null;

        public SolverWorkflow(
            Action stateHasChanged,
            AstroClient client)
        {
            this.stateHasChanged = stateHasChanged;
            this.client = client;
        }

        public SolverWorkflowStage Stage { get; private set; } = SolverWorkflowStage.LoadComponents;

        public bool Loading { get; private set; }

        public IGetInventory_Inventory? Target { get; set; }

        public IGetInventory_Inventory? Sensor { get; set; }

        public IGetInventory_Inventory[] SelectedInventory { get; set; }
            = Array.Empty<IGetInventory_Inventory>();

        public IGetInventory_Inventory[] Inventory { get; private set; }
            = Array.Empty<IGetInventory_Inventory>();

        public IGetInventory_Inventory[] Targets { get; private set; }
            = Array.Empty<IGetInventory_Inventory>();

        public IGetInventory_Inventory[] Sensors { get; private set; }
            = Array.Empty<IGetInventory_Inventory>();

        public void Ready()
        {
            if (ready is not null)
            {
                ready();
            }
        }

        public void RefreshUi() => stateHasChanged();

        public IEnumerable<IWorkflowStep> SolverWorkflowSteps()
        {
            var loadComponents = new WorkflowAction(async () =>
            {
                Loading = true;
                stateHasChanged();
                var results = await client.GetInventory.ExecuteAsync();
                Loading = false;
                if (results != null && results.Data != null)
                {
                    Inventory = results.Data.Inventory
                        .Where(c => c.TargetDirectionConnectionType != ConnectionTypes.Terminator &&
                            c.SensorDirectionConnectionType != ConnectionTypes.Terminator)
                        .ToArray();

                    Targets = results.Data.Inventory
                        .Where(c => c.TargetDirectionConnectionType == ConnectionTypes.Terminator)
                        .ToArray();

                    Target = Targets.FirstOrDefault();

                    Sensors = results.Data.Inventory
                        .Where(c => c.SensorDirectionConnectionType == ConnectionTypes.Terminator)
                        .ToArray();

                    Sensor = Sensors.FirstOrDefault();
                }
                stateHasChanged();
            }, true);

            var chooseParameters = new WorkflowAction(() =>
        {
            Stage = SolverWorkflowStage.ChooseParameters;
            stateHasChanged();
        }, false);

            var solveSolution = new WorkflowAction(() =>
            {
                Stage = SolverWorkflowStage.Solving;
                stateHasChanged();
            }, false);

            yield return loadComponents;
            ready = () => chooseParameters.Yielded();
            yield return chooseParameters;            
            yield return solveSolution;
        }
    }
}
