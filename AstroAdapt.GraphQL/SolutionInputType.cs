using AstroAdapt.Models;

namespace AstroAdapt.GraphQL
{
    /// <summary>
    /// Solution input for GraphQL.
    /// </summary>
    public class SolutionInputType : InputObjectType<Solution>
    {
        /// <summary>
        /// Configure the input to ignore computed properties.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        protected override void Configure(IInputObjectTypeDescriptor<Solution> descriptor)
        {
            descriptor.Ignore(s => s.Deviance);
            descriptor.Ignore(s => s.ComponentCount);            
        }
    }
}
