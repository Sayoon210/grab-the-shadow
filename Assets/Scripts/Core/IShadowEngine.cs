using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Computes a caster's shadow polygon from a light position. Pure contract —
    /// the current implementation is a simple ground projection; it will later be
    /// swapped for a rotational-sweep implementation without touching consumers
    /// (개발기획서 §2.1). Kept free of config/ScriptableObject types so it stays
    /// trivially unit-testable.
    /// </summary>
    public interface IShadowEngine
    {
        /// <param name="lightPosition">World-space sun/light position.</param>
        /// <param name="caster">The occluder casting the shadow.</param>
        /// <param name="groundY">World Y of the ground line the shadow falls on.</param>
        /// <param name="maxLength">Clamp so a low/level light can't cast an infinite shadow.</param>
        ShadowPolygon ComputeShadow(Vector2 lightPosition, in ShadowCaster caster, float groundY, float maxLength);
    }
}
