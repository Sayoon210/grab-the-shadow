using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The computed shadow as a world-space polygon. This single polygon is the
    /// shared source of truth: it is both what gets rendered and (later) what
    /// stomp judgment overlaps against (개발기획서 §2.1–2.2).
    /// </summary>
    public readonly struct ShadowPolygon
    {
        /// <summary>World-space vertices, ordered around the polygon boundary.</summary>
        public readonly Vector2[] Points;
        public readonly int OwnerId;

        public ShadowPolygon(Vector2[] points, int ownerId)
        {
            Points = points;
            OwnerId = ownerId;
        }

        public bool IsValid => Points != null && Points.Length >= 3;
    }
}
