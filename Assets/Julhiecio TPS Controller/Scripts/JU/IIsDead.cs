namespace JU
{
    /// <summary>
    /// Defines a property to verify if the object is dead, useful for objects that have health.
    /// </summary>
    public interface IIsDead
    {
        /// <summary>
        /// Return true if is dead.
        /// </summary>
        bool IsDead { get; }
    }
}