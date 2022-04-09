using AstroAdapt.Models;

namespace AstroAdapters.Services
{
    public class SolverWizard : ISolverWizard
    {
        private readonly IStatusLogger logger;
        private readonly IChannel channel;
        private Task? solving;
        private DateTime? last;
        private StatTracker? statTracker;

        private SolutionDomain? sd;
        private SolutionStage stage;
        
        private SolverStats? stats;

        
        public SolutionStage Stage
        {
            get => stage;
            set
            {
                if (value != stage)
                {
                    stage = value;
                    channel.Publish<ISolverWizard>(nameof(SolutionStage), this);
                }
            }
        }

        public Component? Target { get; private set; }

        public Component? Sensor { get; private set; }

        public IList<Component> Connectors { get; private set; } = new List<Component>();

        public IList<Component> SelectedConnectors { get; private set; } = new List<Component>();

        public IList<Solution> Solutions { get; private set; } = new List<Solution>();

        public SolverStats? Stats => stats;

        public SolverWizard(IChannel channel, IAppHost appHost, IStatusLogger logger)
        {
            this.logger = logger;
            this.channel = channel;
            Stage = SolutionStage.Begin;
            Task.Run(async () =>
            {
                ((List<Component>)Connectors).AddRange(await appHost.GetComponentsAsync());
                Stage = SolutionStage.SetTarget;               
            });
        }

        public void SetConnectors(IList<Component> connectors)
        {
            SelectedConnectors.Clear();
            ((List<Component>)SelectedConnectors).AddRange(connectors);            
        }

        public void SetSensor(Component? sensor)
        {
            Sensor = sensor;
            logger.LogStatus($"Sensor set to: {sensor}");
            if (SelectedConnectors.Count < 1)
            {
                Stage = SolutionStage.SetConnectors;
            }
        }

        public void SetTarget(Component? target)
        {
            Target = target;
            logger.LogStatus($"Target set to: {target}");
            if (Sensor == null)
            {
                Stage = SolutionStage.SetSensor;
            }
        }

        public void SetStage(SolutionStage stage)
        {
            Stage = stage;
            if (stage == SolutionStage.Ready)
            {
                stats = new SolverStats(Target!, Sensor!);
                logger.LogStatus($"Ready to solve image train from {Target} to {Sensor}");
                sd = new SolutionDomain(1);
                sd.SolutionChanged += Sd_SolutionChanged;
                statTracker = new StatTracker(sd, StatUpdate);
                stats = new SolverStats(Target!, Sensor!);
                solving = Task.Run(async () =>
                {
                    SetStage(SolutionStage.Running);
                    last = DateTime.Now;
                    await sd.SolveAsync(SelectedConnectors, Target!, Sensor!, delayForUI: true);
                    Solutions = new List<Solution>(sd.GetSolutions());
                    SetStage(SolutionStage.Solved);
                    solving!.Dispose();
                });
            }
        }

        private void Sd_SolutionChanged(object? sender, SolutionEventArgs e)
        {
            if (e.Solution != null)
            {
                stats!.Solutions.Add(e.Solution!);
            }
            
            if (e.EventType == SolutionEventTypes.SolvingFinished)
            {
                Solutions = new List<Solution>(sd!.GetSolutions());
                SetStage(SolutionStage.Solved);
                solving!.Dispose();
            }
        }

        private void StatUpdate(StatTracker obj)
        {
            if (DateTime.Now - last > TimeSpan.FromMilliseconds(10))
            {
                logger.LogStatus($"Solving... last event: {obj.LastEvent}");
                stats!.Refresh(obj);
                channel.Publish(nameof(StatTracker), obj);
                last = DateTime.Now;
            }
        }
    }
}
