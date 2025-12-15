namespace JU
{
    /// <summary>
    /// Defines a property to verify if the object is ragdolled. Useful for objects that have
    /// physics, like a humanoid character that can fall with ragdoll simulation.
    /// </summary>
    public interface IIsOnRagdoll
    {
        /// <summary>
        /// Return true if is ragdolled.
        /// </summary>
        bool IsOnRagdoll { get; }
    }
}