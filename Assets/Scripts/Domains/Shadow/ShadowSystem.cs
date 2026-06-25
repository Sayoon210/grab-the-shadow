using Game.Core;
using VContainer.Unity;

namespace Game.Shadow
{
    /// <summary>
    /// Drives the shadow each frame: reads the light position, asks the engine for
    /// the polygon, hands it to the view to render. Registered as a VContainer
    /// entry point so timing is container-controlled, not scattered across Unity
    /// magic methods (개발기획서 §4.7).
    /// Note: Tick (per-frame) is fine while the shadow is only visual; once it
    /// feeds stomp judgment this moves to FixedTick for determinism.
    /// </summary>
    public sealed class ShadowSystem : ITickable
    {
        readonly ISunPosition _sun;
        readonly IShadowEngine _engine;
        readonly ShadowView _view;
        readonly IShadowConfig _config;

        public ShadowSystem(ISunPosition sun, IShadowEngine engine, ShadowView view, IShadowConfig config)
        {
            _sun = sun;
            _engine = engine;
            _view = view;
            _config = config;
        }

        public void Tick()
        {
            var poly = _engine.ComputeShadow(
                _sun.WorldPosition, _view.Caster, _config.GroundY, _config.MaxShadowLength);
            _view.Render(poly);
        }
    }
}
