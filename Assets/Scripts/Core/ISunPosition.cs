using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The light source position the shadow system projects from.
    /// Implementations decide *how* the position is produced (a manually-placed
    /// scene object now, an orbit computation later) — consumers only read it.
    /// </summary>
    public interface ISunPosition
    {
        /// <summary>World-space position of the light/sun.</summary>
        Vector2 WorldPosition { get; }
    }
}
