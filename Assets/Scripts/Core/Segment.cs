using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// A world-space line segment. Used as occluder geometry the shadow sweep
    /// clips against (platform edges, ground) — see <see cref="IOccluderSource"/>.
    /// </summary>
    public readonly struct Segment
    {
        public readonly Vector2 A;
        public readonly Vector2 B;

        public Segment(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }
    }
}
