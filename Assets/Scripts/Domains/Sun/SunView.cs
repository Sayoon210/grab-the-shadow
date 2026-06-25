using Game.Core;
using UnityEngine;

namespace Game.Sun
{
    /// <summary>
    /// Thin view that exposes a scene object's transform as the light position.
    /// Place this on a GameObject and move it (in the editor, even during Play)
    /// to drive the shadow system. No logic — when orbiting is added, a plain-C#
    /// SunOrbitService will implement <see cref="ISunPosition"/> instead and this
    /// becomes just the visual marker (개발기획서 §2.1).
    /// </summary>
    public sealed class SunView : MonoBehaviour, ISunPosition
    {
        public Vector2 WorldPosition => transform.position;
    }
}
