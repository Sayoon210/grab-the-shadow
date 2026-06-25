using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Shadow
{
    /// <summary>
    /// Thin view: it is a caster (its transform = caster center) and it renders the
    /// resulting shadow polygon as a mesh. All geometry math lives in the engine;
    /// this only does Unity IO (read transform, write mesh). Implements
    /// <see cref="IShadowCaster"/> so the system can treat it as one of N casters.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShadowCaster : MonoBehaviour, IShadowCaster
    {
        [Header("Caster")]
        [SerializeField] Vector2 _size = new(1f, 2f);
        [SerializeField] int _ownerId;

        [Header("Render")]
        [Tooltip("Optional. If empty, a semi-transparent dark material is created at runtime.")]
        [SerializeField] Material _material;
        [SerializeField] Color _color = new(0f, 0f, 0f, 0.5f);

        Mesh _mesh;
        MeshFilter _filter;
        MeshRenderer _renderer;
        readonly List<int> _tris = new();

#if UNITY_EDITOR
        ShadowPolygon _lastPoly; // for debug gizmo
#endif

        /// <summary>Current caster shape, read fresh from the transform.</summary>
        public CasterShape Shape => new(transform.position, _size, _ownerId);

        void Awake()
        {
            // Keep the mesh in world space on its own object so the caster's
            // transform doesn't offset the already-world-space polygon.
            var holder = new GameObject($"{name}_ShadowMesh");
            holder.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            _filter = holder.AddComponent<MeshFilter>();
            _renderer = holder.AddComponent<MeshRenderer>();
            _mesh = new Mesh { name = "Shadow" };
            _filter.mesh = _mesh;

            if (_material == null)
                _material = new Material(Shader.Find("Sprites/Default"));
            _material.color = _color;
            _renderer.sharedMaterial = _material;
        }

        public void Render(in ShadowPolygon poly)
        {
#if UNITY_EDITOR
            _lastPoly = poly; // debug
#endif
            if (!poly.IsValid)
            {
                _mesh.Clear();
                return;
            }

            int n = poly.Points.Length;
            var verts = new Vector3[n];
            for (int i = 0; i < n; i++)
                verts[i] = poly.Points[i];

            Triangulate(poly.Points, _tris); // concave-safe (sweep shadows aren't always convex)

            _mesh.Clear();
            _mesh.vertices = verts;
            _mesh.SetTriangles(_tris, 0);
            _mesh.RecalculateBounds();
        }

        /// <summary>
        /// Ear-clipping triangulation of a simple polygon, double-sided so winding
        /// and back-face culling never hide the shadow.
        /// </summary>
        static void Triangulate(Vector2[] p, List<int> tris)
        {
            tris.Clear();
            int n = p.Length;
            if (n < 3) return;

            var idx = new List<int>(n);
            if (SignedArea(p) >= 0f)
                for (int i = 0; i < n; i++) idx.Add(i);
            else
                for (int i = n - 1; i >= 0; i--) idx.Add(i); // normalize to CCW

            int guard = 0;
            while (idx.Count > 3 && guard++ < 4096)
            {
                bool clipped = false;
                int cnt = idx.Count;
                for (int i = 0; i < cnt; i++)
                {
                    int ia = idx[(i - 1 + cnt) % cnt];
                    int ib = idx[i];
                    int ic = idx[(i + 1) % cnt];
                    Vector2 a = p[ia], b = p[ib], c = p[ic];

                    float cross = Cross(b - a, c - b);

                    // Drop duplicate/collinear vertices. They form a zero-area "ear"
                    // the reflex test rejects, which otherwise stalls the clip on a
                    // straight edge (e.g. the flat far edge along the ground). Removing
                    // them doesn't change the polygon's straight edges.
                    if (cross > -1e-7f && cross < 1e-7f)
                    {
                        idx.RemoveAt(i);
                        clipped = true;
                        break;
                    }

                    if (cross < 0f) continue; // reflex vertex, not an ear

                    bool contains = false;
                    for (int j = 0; j < cnt; j++)
                    {
                        int jj = idx[j];
                        if (jj == ia || jj == ib || jj == ic) continue;
                        if (PointInTriangle(p[jj], a, b, c)) { contains = true; break; }
                    }
                    if (contains) continue;

                    AddTri(tris, ia, ib, ic);
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break; // degenerate/self-intersecting: stop gracefully
            }

            if (idx.Count == 3)
                AddTri(tris, idx[0], idx[1], idx[2]);
        }

        static void AddTri(List<int> tris, int a, int b, int c)
        {
            tris.Add(a); tris.Add(b); tris.Add(c); // front
            tris.Add(a); tris.Add(c); tris.Add(b); // back
        }

        static float SignedArea(Vector2[] p)
        {
            float sum = 0f;
            for (int i = 0; i < p.Length; i++)
            {
                Vector2 a = p[i], b = p[(i + 1) % p.Length];
                sum += a.x * b.y - b.x * a.y;
            }
            return 0.5f * sum;
        }

        static bool PointInTriangle(Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(b - a, pt - a);
            float d2 = Cross(c - b, pt - b);
            float d3 = Cross(a - c, pt - c);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

        void OnDestroy()
        {
            if (_filter != null) Destroy(_filter.gameObject);
        }

#if UNITY_EDITOR
        // DEBUG: draws the exact polygon the engine produced — vertices (red) and
        // outline (yellow) — so we can see whether the far edge notches at occluders
        // or just connects straight.
        void OnDrawGizmos()
        {
            if (!_lastPoly.IsValid) return;
            var pts = _lastPoly.Points;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < pts.Length; i++)
            {
                Vector3 p = pts[i];
                Vector3 q = pts[(i + 1) % pts.Length];
                Gizmos.DrawLine(p, q);
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < pts.Length; i++)
                Gizmos.DrawSphere(pts[i], 0.08f);
        }
#endif
    }
}
