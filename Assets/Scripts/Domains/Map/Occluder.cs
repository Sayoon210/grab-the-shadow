using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Map
{
    /// <summary>
    /// A box-shaped occluder (platform/ground) that shadows clip against. Thin view:
    /// its four edges in world space are derived from the transform + size. Place
    /// these in the scene as children of an <see cref="OccluderField"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Occluder : MonoBehaviour
    {
        [SerializeField] Vector2 _size = new(3f, 0.5f);

        /// <summary>Appends this box's four edges (world space) to the list.</summary>
        public void AppendEdges(List<Segment> into)
        {
            Vector2 h = _size * 0.5f;
            Vector2 c = transform.position;

            Vector2 bl = c + new Vector2(-h.x, -h.y);
            Vector2 br = c + new Vector2(h.x, -h.y);
            Vector2 tr = c + new Vector2(h.x, h.y);
            Vector2 tl = c + new Vector2(-h.x, h.y);

            into.Add(new Segment(bl, br)); // bottom
            into.Add(new Segment(br, tr)); // right
            into.Add(new Segment(tr, tl)); // top
            into.Add(new Segment(tl, bl)); // left
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position, _size);
        }
#endif
    }
}
