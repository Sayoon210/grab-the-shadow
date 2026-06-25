using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Computes a single caster's shadow polygon from a light position, clipped by
    /// occluder geometry. Pure contract — kept free of config/ScriptableObject and
    /// of any "how many casters" assumption (one caster per call) so it stays
    /// trivially unit-testable and trivially scalable to N casters (개발기획서 §2.1).
    /// </summary>
    public interface IShadowEngine
    {
        /// <param name="lightPosition">World-space sun/light position.</param>
        /// <param name="caster">The occluder casting the shadow.</param>
        /// <param name="occluders">Surfaces the shadow lands on / is clipped by.</param>
        /// <param name="maxLength">Clamp so a ray hitting nothing can't cast an infinite shadow.</param>
        ShadowPolygon ComputeShadow(
            Vector2 lightPosition, in ShadowCaster caster, IReadOnlyList<Segment> occluders, float maxLength);
    }
}
