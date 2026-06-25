using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Map
{
    /// <summary>
    /// Aggregates all child <see cref="Occluder"/> boxes into the occluder segment
    /// list the shadow engine clips against. Registered as <see cref="IOccluderSource"/>.
    /// Rebuilds edges on each access so moving platforms update shadows live.
    /// (Minimal Stage-1 map; real platform geometry / wrap comes later.)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OccluderField : MonoBehaviour, IOccluderSource
    {
        readonly List<Segment> _segments = new();
        Occluder[] _occluders;

        void Awake() => _occluders = GetComponentsInChildren<Occluder>();

        public IReadOnlyList<Segment> Segments
        {
            get
            {
                // Re-gather every access so children added/moved in the editor count.
                _occluders = GetComponentsInChildren<Occluder>();

                _segments.Clear();
                foreach (var o in _occluders)
                    if (o != null) o.AppendEdges(_segments);

                return _segments;
            }
        }

#if UNITY_EDITOR
        // DEBUG: draws the segments the engine ACTUALLY receives (green). If a box
        // shows its blue Occluder gizmo but no green line over it, it is NOT a child
        // of this OccluderField → not registered → shadows ignore it.
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            var segs = Segments;
            for (int i = 0; i < segs.Count; i++)
                Gizmos.DrawLine(segs[i].A, segs[i].B);
        }
#endif
    }
}
