using AstroAdapt.BlazorWasm.GraphQL;

namespace AstroAdapt.BlazorWasm.Workflow
{
    public interface ISolverWorkflowState
    {
        SolverWorkflowStage Stage { get; }
        bool Loading { get; set; }
        IGetInventory_Inventory? Target { get; set; }
        IGetInventory_Inventory? Sensor { get; set; }
        IGetInventory_Inventory[] SelectedInventory { get; set; }
        IGetInventory_Inventory[] Inventory { get; }
        IGetInventory_Inventory[] Targets { get; }
        IGetInventory_Inventory[] Sensors { get; }
        Action Ready { get; }
        Action RefreshUi { get; }
    }
}
