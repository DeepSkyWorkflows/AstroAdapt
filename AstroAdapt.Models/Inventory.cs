using System.Net.Http.Headers;

namespace AstroAdapt.Models
{
    /// <summary>
    /// Available components for a configuration run.
    /// </summary>
    public class Inventory
    {
        private readonly List<Component> components = new ();
        private readonly List<Component> available = new ();
        private readonly List<Component> consumed = new ();

        /// <summary>
        /// Gets a queryable list of available components.
        /// </summary>
        public IQueryable<Component> Available => new List<Component>(available).AsQueryable();
       
        /// <summary>
        /// Instantiates a new instance of inventory.
        /// </summary>
        public Inventory()
        {

        }

        /// <summary>
        /// Instantiates a new instance of inventory.
        /// </summary>
        /// <param name="list">An existing list.</param>
        public Inventory(IEnumerable<Component> list)
        {
            components.AddRange(list);
            available.AddRange(list);
        }

        /// <summary>
        /// Add a new component to inventory.
        /// </summary>
        /// <param name="component">The <see cref="Component"/> to add.</param>
        /// <returns>A value indicating whether the add operation was successful.</returns>
        public bool Add(Component component)
        {
            if (components.Contains(component))
            {
                return false;
            }

            components.Add(component);
            available.Add(component);
            return true;
        }

        /// <summary>
        /// Consume an inventory item.
        /// </summary>
        /// <param name="component">The <see cref="Component"/> to consume.</param>
        /// <returns>A value indicating whether the add operation was successful.</returns>
        public bool Consume(Component component)
        {
            if (!components.Contains(component) ||
                consumed.Contains(component))
            {
                return false;
            }
            consumed.Add(component);
            available.Remove(component);
            return true;
        }

        /// <summary>
        /// Clones for an alternate solution.
        /// </summary>
        /// <param name="ignore">The <see cref="Component"/> to ignore.</param>
        /// <returns>The cloned inventory.</returns>
        public Inventory Clone(Component? ignore = null)
        {
            var clone = new Inventory();
            foreach (var component in components)
            {
                if (ignore != null && component.Equals(ignore))
                {
                    continue;
                }

                clone.Add(component);
                if (consumed.Contains(component))
                {
                    clone.Consume(component);
                }
            }
            return clone;
        }        
    }
}
