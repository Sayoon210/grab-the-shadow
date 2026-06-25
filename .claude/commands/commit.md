# /commit

Create a git commit following this project's conventions.

## Before staging

1. Run `git diff` and `git status` to review all changes.
2. **Separate code and doc changes** — `.md` / `.txt` / asset-only changes commit separately from code; never mix.
3. Split unrelated concerns into multiple commits.
4. Commit on a `<prefix>/<topic>` branch, never straight to `main`.

## Prefix (required on every commit title)

| Prefix | Use when |
|--------|----------|
| `feat:` | new gameplay feature or system |
| `add:` | new file / class / component (not a full feature) |
| `fix:` | bug fix |
| `refactor:` | structure change, no behaviour change |
| `config:` | ScriptableObject, settings, or tunable values |
| `arch:` | DI wiring, LifetimeScope, asmdef changes |
| `docs:` | markdown / documentation only |
| `chore:` | build, packages, project settings |

## Commit message format

```
<prefix>: <short imperative title>          ← imperative, English, ≤72 chars, no period

Why:
  <Why was this change necessary? What problem or design decision drove it?>  (mandatory)

What changed:
  - <Concrete things: class / file / interface affected>
  - ...

How it works (if non-obvious):
  <Behaviour or flow a reader can't guess from the diff. Skip if obvious.>
```

No filler ("as discussed", "small update", "WIP"). Doc commits use the same format with the `docs:` prefix.

## Example

```
feat: add rotational-sweep shadow polygon engine

Why:
  Core shadow mechanic requires a per-character shadow polygon derived from
  a point-light (sun) position and scene occluders. Unity's built-in shadow
  system does not expose the polygon, so a custom engine is needed for both
  rendering and stomp-judgment (they share the same polygon data).

What changed:
  - Added IShadowEngine interface in Core asmdef (input: sun pos + occluder list;
    output: shadow polygon with owner ID)
  - Implemented RotationalSweepShadowEngine (pure C#, no MonoBehaviour)
  - ShadowRenderer reads IShadowEngine output and writes to a MeshFilter
  - ShadowCaster (MonoBehaviour, thin) calls engine each FixedUpdate and
    forwards result to renderer and IStompJudge

How it works:
  Occluder endpoints are angle-sorted around the character. The sweep line
  tracks the nearest wall; each time the nearest wall changes, a triangle is
  closed and a new one begins. The accumulated triangles form the shadow mesh.
```

## Branch strategy

Lightweight GitHub Flow — solo dev, but treat history as portfolio material.

- **`main` is the only long-lived branch.** Must always build and be playable; never commit broken code to it.
- **Work on short-lived branches** named `<prefix>/<topic>` (same prefixes as commits), e.g. `feat/shadow-engine`, `arch/di-skeleton`, `config/physics-tuning`, `docs/dev-plan`.
- **Merge with `git merge --no-ff`** — keeps every in-branch commit *and* records the fork/merge shape in the graph. Do NOT squash (it collapses the in-branch commits).
- **Delete the branch after merge.** Safe: `--no-ff` leaves the commits reachable from `main` via the merge commit, so deleting the label loses nothing.
- **Stages are tags, not branches** — `git tag stage-0`, `git tag stage-1`, … at each milestone. A stage as a long-lived branch only diverges from `main` and causes merge pain.
