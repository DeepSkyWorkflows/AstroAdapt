namespace AstroAdapt.Models
{
    /// <summary>
    /// Receivers are compatible with Extruders
    /// </summary>
    public enum ConnectionTypes
    {
        /// <summary>
        /// Dual can both insert and receive
        /// </summary>
        Dual,

        /// <summary>
        /// Terminates a side of the image train
        /// </summary>
        Terminator,

        /// <summary>
        /// Inserter is a tube or exposed threads that insert into a receiver
        /// </summary>
        Inserter,        

        /// <summary>
        /// Receiver is a hole that may or may not contain threads
        /// </summary>
        Receiver        
    }
}
