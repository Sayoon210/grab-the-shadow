using Game.Core;
using UnityEngine;

namespace Game.Shadow
{
    /// <summary>
    /// Thin view: it is the caster (its transform = caster center) and it renders
    /// the resulting shadow polygon as a mesh. All geometry math lives in the
    /// engine; this only does Unity IO (read transform, write mesh).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShadowView : MonoBehaviour
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

        /// <summary>Current caster description, read fresh from the transform.</summary>
        public ShadowCaster Caster => new(transform.position, _size, _ownerId);

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
            if (!poly.IsValid)
            {
                _mesh.Clear();
                return;
            }

            int n = poly.Points.Length;
            var verts = new Vector3[n];
            for (int i = 0; i < n; i++)
                verts[i] = poly.Points[i];

            // Convex fan, double-sided so winding/culling never hides it.
            var tris = new int[(n - 2) * 6];
            int t = 0;
            for (int i = 1; i < n - 1; i++)
            {
                tris[t++] = 0; tris[t++] = i; tris[t++] = i + 1;       // front
                tris[t++] = 0; tris[t++] = i + 1; tris[t++] = i;       // back
            }

            _mesh.Clear();
            _mesh.vertices = verts;
            _mesh.triangles = tris;
            _mesh.RecalculateBounds();
        }

        void OnDestroy()
        {
            if (_filter != null) Destroy(_filter.gameObject);
        }
    }
}
