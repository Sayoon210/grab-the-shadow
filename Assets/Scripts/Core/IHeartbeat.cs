namespace Game.Core
{
    /// <summary>
    /// TEMPORARY bootstrap probe contract.
    /// Exists only to prove the intended dependency path in miniature:
    /// the contract lives in <c>Game.Core</c>, while its implementation and
    /// wiring live in <c>Game.App</c> (App → Core, never the reverse).
    /// Delete once the first real domain contract (e.g. ISunPosition) replaces it.
    /// </summary>
    public interface IHeartbeat
    {
        /// <summary>Number of heartbeats elapsed since boot.</summary>
        int Beats { get; }
    }
}
