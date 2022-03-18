namespace AstroAdapt.Models
{
    /// <summary>
    /// Job for solution.
    /// </summary>
    /// <param name="flags">The flags represent current state.</param>
    /// <param name="solution">The current solution chain.</param>
    public record SolverJob(byte[] flags, (Guid, bool)[] solution);    
}
