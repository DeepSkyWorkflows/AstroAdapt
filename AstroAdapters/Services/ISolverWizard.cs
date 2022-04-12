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

        Solution? LastSolution { get; set; }
        SolverStats? Stats { get; }
        double BackfocusTolerance { get; set; }

        int StopAfterNSolutions { get; set; }

        int StopAfterNPerfectSolutions { get; set; }

        int MaxConnectors { get; set; }

        void SetTarget(Component? target);
        void SetSensor(Component? sensor);
        void SetConnectors(IList<Component> connectors);
        void SetStage(SolutionStage stage);
    }
}
