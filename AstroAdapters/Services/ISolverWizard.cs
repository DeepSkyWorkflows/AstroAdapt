using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public interface ISolverWizard
    {
        SolutionStage Stage { get; }
        Component? Target { get; }
        Component? Sensor { get; }
        IList<Component> Connectors{ get; }
        IList<Component> SelectedConnectors { get; }
        IList<Solution> Solutions { get; }
        SolverStats? Stats { get; }

        void SetTarget(Component? target);
        void SetSensor(Component? sensor);
        void SetConnectors(IList<Component> connectors);
        void SetStage(SolutionStage stage);
    }
}
