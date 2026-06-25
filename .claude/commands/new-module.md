# /new-module

Scaffold a new DI domain module (e.g. Sun, Vision, Round) with correct asmdef
direction, interface-first design, and VContainer registration. Follow the steps
in order — each one prevents a CLAUDE.md rule violation.

## Steps

1. **Folder** — `Domains/<Name>/` with subfolders as needed (`Runtime/`, `Config/`).
2. **asmdef** — `Game.<Name>.asmdef` referencing **only** `Game.Core` (+ third-party wrappers it needs). Verify direction: `Core` references nothing back; `App` references this module. Never reference a sibling domain directly — go through a `Core` interface.
3. **Interface first** — define the contract before the implementation. Keep it small and role-based (ISP).
   - **Cross-domain** contract (another module consumes it) → put it in `Core`.
   - **Domain-internal** contract → keep it in this module.
4. **Logic** — implement as **plain C#**, no MonoBehaviour. Constructor-injected deps only. This is the testable core.
5. **Config** — if it has tunables, make a `ScriptableObject` (`<Name>Config`) holding them. No magic numbers in code.
6. **View (only if Unity IO is needed)** — a **thin** MonoBehaviour that does transform/render/input IO and forwards to the logic. No gameplay logic here.
7. **Register** in the LifetimeScope matching its **lifetime** (see table), via an Installer:
   - logic: `builder.Register<Impl>(Lifetime.Singleton).As<IContract>();`
   - config SO: `builder.RegisterInstance(configAsset).As<I…Config>();` (asset wired in inspector)
   - view MonoBehaviour: `builder.RegisterComponentInHierarchy<View>();`
   - per-frame/init logic: register as entry point — `builder.RegisterEntryPoint<Impl>();` (IStartable / ITickable / IFixedTickable)
8. **Lifecycle via entry points, not Unity magic methods.** Init in `Start()` (IStartable), per-frame in `ITickable`/`IFixedTickable`. Never put gameplay timing in a MonoBehaviour `Update`.

## Where does it register? (scope = lifetime)

| Scope | Lifetime | Put here |
|-------|----------|----------|
| `Root` | whole app | Config, global services, Input |
| `Game` | one match | Sun, Round, match-scoped services |
| `Player` | one character | Movement, Skill, Stats (×N players) |

## Example — Sun domain

```
Domains/Sun/
  Sun.asmdef                  → references Game.Core only
  Runtime/
    SunOrbitService.cs        → plain C#, ITickable; computes sun pos over time
    SunView.cs                → thin MonoBehaviour; moves the Light2D transform
  Config/
    SunConfig.cs              → SO: orbitPeriod, radius, startAngle

Core/
  ISunPosition.cs             → cross-domain contract (Shadow & Vision consume it)
```

```csharp
// Core/ISunPosition.cs  — small, role-based; lives in Core so other domains can read it
public interface ISunPosition {
    Vector2 WorldPosition { get; }
    float    Angle { get; }
}

// Domains/Sun/Runtime/SunOrbitService.cs  — plain C#, testable, no MonoBehaviour
public sealed class SunOrbitService : ISunPosition, IFixedTickable {
    readonly ISunConfig _cfg;
    public SunOrbitService(ISunConfig cfg) => _cfg = cfg;

    public Vector2 WorldPosition { get; private set; }
    public float    Angle        { get; private set; }

    public void FixedTick() {
        Angle = (Angle + _cfg.AngularSpeed * Time.fixedDeltaTime) % 360f;
        WorldPosition = OrbitPoint(Angle, _cfg.Radius);
    }
}

// GameLifetimeScope.cs  — Sun lives one match → Game scope
protected override void Configure(IContainerBuilder builder) {
    builder.RegisterInstance(_sunConfig).As<ISunConfig>();        // SO from inspector
    builder.Register<SunOrbitService>(Lifetime.Singleton)
           .AsImplementedInterfaces();                            // ISunPosition + IFixedTickable
    builder.RegisterComponentInHierarchy<SunView>();              // thin view
}
```

## Gotchas

- New asmdef → Unity needs a recompile before the namespace resolves; expect a domain reload.
- Forgetting `.As<IContract>()` registers the concrete only → consumers asking for the interface fail to resolve.
- A domain referencing another domain's asmdef = direction violation. If you reach for it, the shared piece belongs in `Core`.
- `RegisterComponentInHierarchy` needs the component present in the scene under that scope; otherwise use `RegisterComponentInNewPrefab` or a factory.
