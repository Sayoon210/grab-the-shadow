using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Supplies the occluder geometry (platform edges, ground) that shadows are
    /// clipped against. Provided by the Map domain; consumed by the shadow engine.
    /// The shadow lands on the nearest occluder along each ray, so "ground" is just
    /// a wide occluder rather than a special case (개발기획서 §2.3).
    /// </summary>
    public interface IOccluderSource
    {
        /// <summary>Current occluder segments in world space (may be rebuilt each frame).</summary>
        IReadOnlyList<Segment> Segments { get; }
    }
}
