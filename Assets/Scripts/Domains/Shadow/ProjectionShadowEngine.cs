using Game.Core;
using UnityEngine;

namespace Game.Shadow
{
    /// <summary>
    /// First-cut shadow engine: projects the caster box's top edge away from the
    /// light onto the ground line, forming a trapezoid. Pure C#, no MonoBehaviour,
    /// no Unity lifecycle — fully unit-testable. This is the interchangeable
    /// implementation behind <see cref="IShadowEngine"/>; a rotational-sweep
    /// version replaces it later with no change to consumers (개발기획서 §2.1).
    /// </summary>
    public sealed class ProjectionShadowEngine : IShadowEngine
    {
        public ShadowPolygon ComputeShadow(Vector2 lightPosition, in ShadowCaster caster, float groundY, float maxLength)
        {
            Vector2 half = caster.Size * 0.5f;
            Vector2 c = caster.Center;

            Vector2 bottomLeft = c + new Vector2(-half.x, -half.y);
            Vector2 bottomRight = c + new Vector2(half.x, -half.y);
            Vector2 topLeft = c + new Vector2(-half.x, half.y);
            Vector2 topRight = c + new Vector2(half.x, half.y);

            Vector2 projLeft = ProjectToGround(topLeft, lightPosition, groundY, maxLength);
            Vector2 projRight = ProjectToGround(topRight, lightPosition, groundY, maxLength);

            // Boundary order: base of the caster -> projected far edge.
            var points = new[] { bottomLeft, bottomRight, projRight, projLeft };
            return new ShadowPolygon(points, caster.OwnerId);
        }

        /// <summary>
        /// Casts a ray from <paramref name="point"/> directly away from the light
        /// and returns where it meets the ground line, clamped to <paramref name="maxLength"/>.
        /// </summary>
        static Vector2 ProjectToGround(Vector2 point, Vector2 light, float groundY, float maxLength)
        {
            Vector2 dir = point - light; // direction away from the light

            // Light must be above the point for the shadow to fall downward onto
            // the ground; otherwise just extend horizontally as a fallback.
            const float epsilon = 1e-4f;
            Vector2 hit;
            if (dir.y < -epsilon)
            {
                float t = (groundY - point.y) / dir.y;
                if (t < 0f) t = 0f;
                hit = point + dir * t;
            }
            else
            {
                hit = point + dir.normalized * maxLength;
            }

            Vector2 offset = hit - point;
            if (offset.sqrMagnitude > maxLength * maxLength)
                hit = point + offset.normalized * maxLength;

            return hit;
        }
    }
}
