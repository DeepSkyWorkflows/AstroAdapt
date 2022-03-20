using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroAdapt.Models
{
    /// <summary>
    /// An item in a solution list.
    /// </summary>
    public class SolutionItem
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public SolutionItem()
        {
        }

        /// <summary>
        /// Creates a new instance from a component. 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="sequence"></param>
        public SolutionItem(Component component, int sequence)
        {
            ComponentId = component.Id;
            ItemName = $"{component.Manufacturer.Name} {component.Model}";
            BackFocusMm = component.BackFocusMm;
            LengthMm = component.LengthMm;
            ThreadRecessMm = component.ThreadRecessMm;
            ComponentType = component.ComponentType;
            TargetDirectionConnectionType = component.TargetDirectionConnectionType;
            TargetDirectionConnectionSize = component.TargetDirectionConnectionSize;
            SensorDirectionConnectionType = component.SensorDirectionConnectionType;
            SensorDirectionConnectionSize = component.SensorDirectionConnectionSize;
            ShortCode = component.ShortCode;
            Sequence = sequence;
        }

        /// <summary>
        /// Gets or sets the unique id of the item.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Order in the soluion.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Gets or sets the guid of the related component.
        /// </summary>
        public Guid ComponentId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the manufacturer and model of the component.
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the distance the thread is recessed in millimeters. This can automatically
        /// reduce the amount of backfocus needed.
        /// </summary>
        public double ThreadRecessMm { get; set; } = 0;

        /// <summary>
        /// Gets or sets the back focus requirement. Backfocus is computed from the component that
        /// is closest to the sensor. 
        /// </summary>
        public double BackFocusMm { get; set; } = 0;

        /// <summary>
        /// Gets or sets the lenth of the component in millimeters.
        /// </summary>
        public double LengthMm { get; set; } = 0;

        /// <summary>
        /// Gets or sets the type of the component
        /// </summary>
        public ComponentTypes ComponentType { get; set; } = ComponentTypes.Other;

        /// <summary>
        /// Gets or sets the connection type facing the imaging target (sky).
        /// </summary>
        public ConnectionTypes TargetDirectionConnectionType { get; set; } = ConnectionTypes.Terminator;

        /// <summary>
        /// Gets or sets the size of the connection for cmpatbility.
        /// </summary>
        public ConnectionSizes TargetDirectionConnectionSize { get; set; } = ConnectionSizes.Zero;

        /// <summary>
        /// Gets or sets the connection type facing the imaging target (sky).
        /// </summary>
        public ConnectionTypes SensorDirectionConnectionType { get; set; } = ConnectionTypes.Terminator;

        /// <summary>
        /// Gets or sets the size of the connection for compatbility.
        /// </summary>
        public ConnectionSizes SensorDirectionConnectionSize { get; set; } = ConnectionSizes.Zero;

        /// <summary>
        /// Code for report printing.
        /// </summary>
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets a string representation of the component.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{TargetDirectionConnectionType.AsVisualString(true)} {TargetDirectionConnectionSize} (");
            sb.Append(ItemName);
            if (ThreadRecessMm > 0)
            {
                sb.Append($"thr={ThreadRecessMm}mm ");
            }

            if (BackFocusMm > 0)
            {
                sb.Append($"bf={BackFocusMm}mm ");
            }

            sb.Append($"len={LengthMm}mm)");
            sb.Append($"{SensorDirectionConnectionSize}{SensorDirectionConnectionType.AsVisualString(false)}");
            return sb.ToString();
        }

    }
}
