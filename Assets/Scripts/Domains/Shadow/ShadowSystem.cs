using Game.Core;
using VContainer.Unity;

namespace Game.Shadow
{
    /// <summary>
    /// Drives the shadow each frame: reads the light position and occluders, asks
    /// the engine for the polygon, hands it to the caster to render. Registered as a
    /// VContainer entry point so timing is container-controlled, not scattered across
    /// Unity magic methods (개발기획서 §4.7).
    ///
    /// Depends on IShadowCaster (single, for now). Scaling to N casters is a local
    /// change here — loop a caster list — with no change to the engine.
    /// Note: Tick (per-frame) is fine while the shadow is only visual; once it feeds
    /// stomp judgment this moves to FixedTick for determinism.
    /// </summary>
    public sealed class ShadowSystem : ITickable
    {
        readonly ISunPosition _sun;
        readonly IShadowEngine _engine;
        readonly IShadowCaster _caster;
        readonly IOccluderSource _occluders;
        readonly IShadowConfig _config;

        public ShadowSystem(
            ISunPosition sun, IShadowEngine engine, IShadowCaster caster,
            IOccluderSource occluders, IShadowConfig config)
        {
            _sun = sun;
            _engine = engine;
            _caster = caster;
            _occluders = occluders;
            _config = config;
        }

        public void Tick()
        {
            var poly = _engine.ComputeShadow(
                _sun.WorldPosition, _caster.Caster, _occluders.Segments, _config.MaxShadowLength);
            _caster.Render(poly);
        }
    }
}
