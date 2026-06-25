# CLAUDE.md

Unity 2D PvP (moving-sun shadow game), also a **programmer portfolio** — architecture quality counts.
Details: `기획서.md` (design) · `개발기획서.md` (dev). Keep this file lean; link, don't duplicate.

## ✅ ALWAYS
- Inject via **VContainer**; depend on **interfaces**, not concretes (keep them small).
- Logic in plain, testable C#; **MonoBehaviours stay thin** (wiring/Unity-IO only).
- **Composition over inheritance.** **One source of truth** (DRY).
- Tunables in **ScriptableObject config**, injected.
- Respect Unity **lifecycle order** (refs in `Awake`, interaction in `Start`, per-frame in `Update`/`FixedUpdate`); init via DI entry points, never implicit timing.
- Dependency direction one-way: `App → domains → Core`; domains never touch views/UI.
- Pure/deterministic logic on a **fixed timestep**. Match surrounding style.

## 🚫 NEVER
- Hardcode magic numbers/tunables → config.
- Singletons / static globals / `Resources.Load` for deps → inject.
- Gameplay logic in `Update` without a service layer.
- Deep inheritance / god base class → compose.
- Concrete cross-module deps → interface. Couple domain to a 3rd-party lib → wrap it.
- Duplicate data/logic. Swallow errors. Over-engineer.

## Architecture
Single entry point (`RootLifetimeScope`) boots everything. Scopes split by **lifetime**:
`Root` (app: Config, services, Input) → `Game` (per match: Sun, Round) → `Player × N` (Character = Movement/Skill/Stats, composed).
Per-domain **asmdef** enforces direction at compile time: `Core ← {Shadow, Character, Sun, Vision, Map, Round, Input, Network} ← App`.
Domains never reference each other — shared contracts live in `Core`; wiring happens only in `App` scopes. (`Map` = platforms/occluders + map wrap + camera; feeds `IOccluderSource` to Shadow.)

## Stack
Unity 2D (URP) · C# · **VContainer** (DI) · **new Input System** (→ command objects) · rotational-sweep shadow polygon.

## Project Decisions *(may change)*
- Shadow = one sweep polygon, shared by renderer + collider. **No raycast** (polygon overlap only).
- Sun = orbiting 2D point light; shadow direction/length follow from its geometry.
- Character = custom **asymmetric-gravity** jump (gravity/velocity derived from height + time-to-peak/descent; in `PhysicsConfig`).
- Vision cone follows the mouse (aim ≠ movement). Networking behind `INetworkService` (dummy for now).

## Current Stage
**Stage 0–1**: DI skeleton + shadow engine + core loop (judge, stomp+cooldown, sun orbit).
**Deferred — do NOT build:** multiplayer, artificial lighting, 2.5D shadows, sound.

---

## ✅ ALWAYS
- Inject via **VContainer**; depend on **interfaces**, not concretes (keep them small).
- Logic in plain, testable C#; **MonoBehaviours stay thin** (wiring/Unity-IO only).
- **Composition over inheritance.** **One source of truth** (DRY).
- Tunables in **ScriptableObject config**, injected.
- Respect Unity **lifecycle order** (refs in `Awake`, interaction in `Start`, per-frame in `Update`/`FixedUpdate`); init via DI entry points, never implicit timing.
- Dependency direction one-way: `App → domains → Core`; domains never touch views/UI.
- Pure/deterministic logic on a **fixed timestep**. Match surrounding style.

## 🚫 NEVER
- Hardcode magic numbers/tunables → config.
- Singletons / static globals / `Resources.Load` for deps → inject.
- Gameplay logic in `Update` without a service layer.
- Deep inheritance / god base class → compose.
- Concrete cross-module deps → interface. Couple domain to a 3rd-party lib → wrap it.
- Duplicate data/logic. Swallow errors. Over-engineer.
