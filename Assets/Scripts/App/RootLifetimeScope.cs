using Game.Core;
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
        builder.RegisterComponentInHierarchy<SunView>().As<ISunPosition>();

        // Shadow: pure engine + scene view + the per-frame driver (entry point).
        builder.Register<ProjectionShadowEngine>(Lifetime.Singleton).As<IShadowEngine>();
        builder.RegisterComponentInHierarchy<ShadowView>();
        builder.RegisterEntryPoint<ShadowSystem>();
    }
}
