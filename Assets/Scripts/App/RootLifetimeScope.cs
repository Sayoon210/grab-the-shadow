using Game.Core;
using Game.Map;
using Game.Shadow;
using Game.Sun;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] ShadowConfig _shadowConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        // Config: data in a ScriptableObject, distributed via DI behind its interface.
        builder.RegisterInstance(_shadowConfig).As<IShadowConfig>();

        // Sun: the movable light object in the scene provides the light position.
        builder.RegisterComponentInHierarchy<SunLightSource>().As<ISunPosition>();

        // Map: occluder geometry the shadow is clipped against.
        builder.RegisterComponentInHierarchy<OccluderField>().As<IOccluderSource>();

        // Shadow: pure sweep engine + scene caster/view + the per-frame driver.
        builder.Register<RotationalSweepShadowEngine>(Lifetime.Singleton).As<IShadowEngine>();
        builder.RegisterComponentInHierarchy<ShadowCaster>().As<IShadowCaster>();
        builder.RegisterEntryPoint<ShadowSystem>();
    }
}
