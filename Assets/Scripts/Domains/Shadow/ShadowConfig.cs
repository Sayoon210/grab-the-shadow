using UnityEngine;

namespace Game.Shadow
{
    /// <summary>Domain-internal tunables for the shadow system (injected, not hardcoded).</summary>
    public interface IShadowConfig
    {
        /// <summary>World Y of the ground line shadows fall onto.</summary>
        float GroundY { get; }

        /// <summary>Maximum shadow length, so a low/level light can't cast infinitely.</summary>
        float MaxShadowLength { get; }
    }

    /// <summary>
    /// ScriptableObject holding shadow tunables. Data lives here (inspector-editable),
    /// distribution is via DI — registered in the LifetimeScope as <see cref="IShadowConfig"/>
    /// (개발기획서 §4.4).
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Shadow Config", fileName = "ShadowConfig")]
    public sealed class ShadowConfig : ScriptableObject, IShadowConfig
    {
        [SerializeField] float _groundY = -3f;
        [SerializeField] float _maxShadowLength = 30f;

        public float GroundY => _groundY;
        public float MaxShadowLength => _maxShadowLength;
    }
}
