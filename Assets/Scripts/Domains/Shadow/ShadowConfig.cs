using UnityEngine;

namespace Game.Shadow
{
    /// <summary>Domain-internal tunables for the shadow system (injected, not hardcoded).</summary>
    public interface IShadowConfig
    {
        /// <summary>Max shadow length for rays that hit no occluder (avoids infinite shadows).</summary>
        float MaxShadowLength { get; }
    }

    /// <summary>
    /// ScriptableObject holding shadow tunables. Data lives here (inspector-editable),
    /// distribution is via DI — registered in the LifetimeScope as <see cref="IShadowConfig"/>
    /// (개발기획서 §4.4). Ground is no longer a single line; it is just an occluder, so
    /// groundY was removed when the sweep engine replaced the projection model.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Shadow Config", fileName = "ShadowConfig")]
    public sealed class ShadowConfig : ScriptableObject, IShadowConfig
    {
        [SerializeField] float _maxShadowLength = 30f;

        public float MaxShadowLength => _maxShadowLength;
    }
}
