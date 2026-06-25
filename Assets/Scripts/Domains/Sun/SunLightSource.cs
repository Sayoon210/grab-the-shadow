using Game.Core;
using UnityEngine;

namespace Game.Sun
{
    /// <summary>
    /// Thin scene light source: exposes a GameObject's transform as the sun position
    /// (the 2D point light of 개발기획서 §2.1). Move it (in the editor, even during
    /// Play) to drive the shadow system. No logic — when orbiting is added, a plain-C#
    /// SunOrbitService will implement <see cref="ISunPosition"/> instead and this
    /// becomes just the visual light source.
    /// </summary>
    public sealed class SunLightSource : MonoBehaviour, ISunPosition
    {
        public Vector2 WorldPosition => transform.position;
    }
}
