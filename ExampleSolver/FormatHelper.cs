using System.Text;
using AstroAdapt.Models;

namespace ExampleSolver
{
    public class FormatHelper
    {
        private readonly Solution solution;
        private int targetSizeCols = 1;
        private int modelCols = 1;
        private int sensorSizeCols = 1;

        public FormatHelper(Solution solution)
        {
            this.solution = solution;
            Prepare();
        }

        private string BuildRow(Component component)
        {
            if (component.TargetDirectionConnectionType == ConnectionTypes.Terminator)
            {
                return TargetRow(component);
            }

            if (component.TargetDirectionConnectionType == ConnectionTypes.Terminator)
            {
                return SensorRow(component);
            }
            var sb = new StringBuilder();
            sb.Append('\t');
            sb.Append(component.TargetDirectionConnectionType.AsVisualString(true));
            sb.Append('\t');
            sb.Append(GetStringForColumnSize(component.TargetDirectionConnectionSize.ToString(),
                targetSizeCols));
            sb.Append(GetStringForColumnSize(component.Model, modelCols));
            sb.Append(GetStringForColumnSize(component.SensorDirectionConnectionSize.ToString(),
                sensorSizeCols));
            sb.Append(component.SensorDirectionConnectionType.AsVisualString(false));
            return sb.ToString();
        }

        private string SensorRow(Component component)
        {
            var sb = new StringBuilder();
            sb.Append('\t');
            sb.Append(component.TargetDirectionConnectionType.AsVisualString(true));
            sb.Append('\t');
            sb.Append(GetStringForColumnSize(component.TargetDirectionConnectionSize.ToString(),
                targetSizeCols));
            sb.Append(GetStringForColumnSize(component.Model, modelCols));
            return sb.ToString();
        }

        private string TargetRow(Component component)
        {
            var sb = new StringBuilder();
            sb.Append("\t \t");
            sb.Append(GetStringForColumnSize(" ",targetSizeCols));
            sb.Append(GetStringForColumnSize(component.Model, modelCols));
            sb.Append(GetStringForColumnSize(component.SensorDirectionConnectionSize.ToString(),
                sensorSizeCols));
            sb.Append(component.SensorDirectionConnectionType.AsVisualString(false));
            return sb.ToString();

        }

        private void Prepare()
        {
            var itemsToConsider = solution.Connections.Union(new[]
            {
                solution.Target, solution.Sensor
            });

            var maxTargetSize = itemsToConsider.Max(c => c.TargetDirectionConnectionSize.ToString().Length);
            targetSizeCols = maxTargetSize / 8 + 1;
            var maxModel = itemsToConsider.Max(c => c.Model.Length);
            modelCols = maxModel / 8 + 1;
            var maxSensorSize = itemsToConsider.Max(c => c.SensorDirectionConnectionSize.ToString().Length);
            sensorSizeCols = maxSensorSize / 8 + 1;
        }

        private static string GetStringForColumnSize(string src, int cols)
        {
            if (src.Length == cols*8)
            {
                return src;
            }
            var diff = (cols * 8) - src.Length;
            var tabs = diff / 8 + 1;
            var tabStr = new string('\t', tabs);
            return $"{src}{tabStr}";
        }

        public string Build()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{solution.Target} TO {solution.Sensor}");
            sb.AppendLine($"BF: {solution.BackFocusMm}\tLen: {solution.LengthMm}\tDeviance: {solution.Deviance} ({solution.DeviancePct * 100}%)");
            foreach (var component in solution.Connections)
            {
                sb.AppendLine(BuildRow(component));
            }
            sb.AppendLine($"{Environment.NewLine}---{Environment.NewLine}");
            return sb.ToString();
        }
    }
}
