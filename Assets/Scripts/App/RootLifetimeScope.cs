using Game.App;
using Game.Core;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Temporary: register the heartbeat probe as a VContainer entry point so
        // its Initialize()/Tick() run. Exposed as IHeartbeat (Core contract) to
        // exercise the App → Core injection path. Replace with real services later.
        builder.RegisterEntryPoint<HeartbeatService>()
               .As<IHeartbeat>();
    }
}
