using Game.Core;
using UnityEngine;
using VContainer.Unity;

namespace Game.App
{
    /// <summary>
    /// TEMPORARY bootstrap probe.
    /// Registered as a VContainer entry point to prove the DI lifecycle is wired
    /// correctly: <see cref="Initialize"/> fires once after all injection completes,
    /// and <see cref="Tick"/> fires every frame. It logs a heartbeat so we can
    /// confirm the (otherwise empty) game loop is actually running.
    /// Delete once real domains (Sun/Shadow/...) drive the loop.
    /// </summary>
    public sealed class HeartbeatService : IHeartbeat, IInitializable, ITickable
    {
        const float Interval = 1f;

        float _elapsed;

        public int Beats { get; private set; }

        public void Initialize()
        {
            Debug.Log("[Heartbeat] RootLifetimeScope booted — DI loop alive.");
        }

        public void Tick()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < Interval) return;

            _elapsed -= Interval;
            Beats++;
            Debug.Log($"[Heartbeat] beat #{Beats}");
        }
    }
}
