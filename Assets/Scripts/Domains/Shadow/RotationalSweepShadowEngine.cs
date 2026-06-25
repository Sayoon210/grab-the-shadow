using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Shadow
{
    /// <summary>
    /// Cast-shadow engine via a rotational sweep (redblobgames visibility) restricted
    /// to the caster's arc.
    ///
    /// Key insight: the shadow's far boundary only changes direction at OCCLUDER
    /// ENDPOINTS — between two endpoints the nearest occluder is constant, so the
    /// boundary is a straight line. So we don't sample rays everywhere; we sort the
    /// occluder endpoints inside the caster's angular range, and at each endpoint
    /// check whether the nearest occluder changed. It only emits far-edge vertices
    /// where that happens (the "cliff" between the old and new nearest surface).
    ///
    /// Model: the caster box subtends an arc [angA, angB] from the light. The near
    /// edge is the caster's silhouette chord a→b; the far edge is where each shadow
    /// ray lands on the nearest occluder behind the caster (or maxLength). Platforms
    /// thus clip the shadow → safe zones. Pure C#, one caster per call.
    /// </summary>
    public sealed class RotationalSweepShadowEngine : IShadowEngine
    {
        const float Eps = 1e-4f;
        const float AngEps = 1e-4f; // angular nudge (radians) to sample just before/after an endpoint

        readonly List<float> _events = new();

        public ShadowPolygon ComputeShadow(
            Vector2 light, in CasterShape caster, IReadOnlyList<Segment> occluders, float maxLength)
        {
            Vector2 hsize = caster.Size * 0.5f;
            Vector2 c = caster.Center;
            if (hsize.x <= Eps && hsize.y <= Eps) return default; // degenerate caster

            Vector2 centerDir = c - light;
            if (centerDir.sqrMagnitude <= Eps * Eps) return default; // light at caster center
            Vector2 uHat = centerDir.normalized;

            // (a) Caster arc: silhouette extremes a (min angle) and b (max angle).
            Vector2 c0 = c + new Vector2(-hsize.x, -hsize.y);
            Vector2 c1 = c + new Vector2(hsize.x, -hsize.y);
            Vector2 c2 = c + new Vector2(hsize.x, hsize.y);
            Vector2 c3 = c + new Vector2(-hsize.x, hsize.y);

            Vector2 a = c0, b = c0;
            float angA = float.MaxValue, angB = float.MinValue;
            ConsiderCorner(light, uHat, c0, ref a, ref angA, ref b, ref angB);
            ConsiderCorner(light, uHat, c1, ref a, ref angA, ref b, ref angB);
            ConsiderCorner(light, uHat, c2, ref a, ref angA, ref b, ref angB);
            ConsiderCorner(light, uHat, c3, ref a, ref angA, ref b, ref angB);
            if (angB - angA <= AngEps) return default;

            // (b) Candidate change angles = occluder endpoints strictly inside the arc.
            _events.Clear();
            for (int i = 0; i < occluders.Count; i++)
            {
                AddEvent(light, uHat, occluders[i].A, angA, angB);
                AddEvent(light, uHat, occluders[i].B, angA, angB);
            }
            _events.Sort();

            // (c) Sweep: walk the arc, emit a far vertex pair only where the nearest
            // occluder changes (cliff between the leaving and arriving surface).
            var far = new List<Vector2>(_events.Count * 2 + 2);

            int startIdx = NearestSeg(light, uHat, angA + AngEps, a, b, occluders, maxLength);
            far.Add(PointAt(light, uHat, angA, startIdx, occluders, maxLength));

            for (int e = 0; e < _events.Count; e++)
            {
                float ang = _events[e];
                int before = NearestSeg(light, uHat, ang - AngEps, a, b, occluders, maxLength);
                int after = NearestSeg(light, uHat, ang + AngEps, a, b, occluders, maxLength);
                if (before == after) continue;

                far.Add(PointAt(light, uHat, ang, before, occluders, maxLength)); // leave old surface
                far.Add(PointAt(light, uHat, ang, after, occluders, maxLength));  // arrive new surface
            }

            int endIdx = NearestSeg(light, uHat, angB - AngEps, a, b, occluders, maxLength);
            far.Add(PointAt(light, uHat, angB, endIdx, occluders, maxLength));

            // (d) Polygon = near edge (chord a→b) + far edge (a-side → b-side).
            var points = new Vector2[2 + far.Count];
            points[0] = a;
            for (int k = 0; k < far.Count; k++) points[1 + k] = far[k];
            points[1 + far.Count] = b;
            return new ShadowPolygon(points, caster.OwnerId);
        }

        static void ConsiderCorner(
            Vector2 light, Vector2 uHat, Vector2 corner,
            ref Vector2 a, ref float angA, ref Vector2 b, ref float angB)
        {
            float ang = RelAngle(uHat, corner - light);
            if (ang < angA) { angA = ang; a = corner; }
            if (ang > angB) { angB = ang; b = corner; }
        }

        void AddEvent(Vector2 light, Vector2 uHat, Vector2 endpoint, float angA, float angB)
        {
            float ang = RelAngle(uHat, endpoint - light);
            if (ang > angA + AngEps && ang < angB - AngEps)
                _events.Add(ang);
        }

        /// <summary>Index of the nearest occluder behind the caster chord at angle <paramref name="ang"/>, or -1.</summary>
        static int NearestSeg(
            Vector2 light, Vector2 uHat, float ang, Vector2 a, Vector2 b,
            IReadOnlyList<Segment> occluders, float maxLength)
        {
            Vector2 dir = Dir(uHat, ang);

            // Shadow must fall behind the caster: only count hits past the chord.
            float chordDist = RaySegment(light, dir, a, b, out float ct) ? ct : 0f;

            int best = -1;
            float bestT = maxLength;
            for (int i = 0; i < occluders.Count; i++)
            {
                if (RaySegment(light, dir, occluders[i].A, occluders[i].B, out float t)
                    && t > chordDist + Eps && t < bestT)
                {
                    bestT = t;
                    best = i;
                }
            }
            return best;
        }

        /// <summary>Far point at angle <paramref name="ang"/> on occluder <paramref name="idx"/> (or maxLength if none).</summary>
        static Vector2 PointAt(
            Vector2 light, Vector2 uHat, float ang, int idx,
            IReadOnlyList<Segment> occluders, float maxLength)
        {
            Vector2 dir = Dir(uHat, ang);
            if (idx >= 0
                && RaySegment(light, dir, occluders[idx].A, occluders[idx].B, out float t)
                && t > Eps)
            {
                return light + dir * Mathf.Min(t, maxLength);
            }
            return light + dir * maxLength;
        }

        /// <summary>Ray (origin + t*dir, t≥0) vs segment [q1,q2]. Outputs t on hit.</summary>
        static bool RaySegment(Vector2 origin, Vector2 dir, Vector2 q1, Vector2 q2, out float t)
        {
            t = 0f;
            Vector2 e = q2 - q1;
            float denom = dir.x * e.y - dir.y * e.x; // cross(dir, e)
            if (Mathf.Abs(denom) < 1e-9f) return false; // parallel

            Vector2 diff = q1 - origin;
            t = (diff.x * e.y - diff.y * e.x) / denom;           // along the ray
            float s = (diff.x * dir.y - diff.y * dir.x) / denom; // along the segment

            // Endpoint tolerance: a ray grazing a segment's corner gives s ≈ 0 or 1,
            // which float error can push just outside [0,1] → a spurious "miss" that
            // makes that ray shoot to maxLength (a spike). Accept a hair past the ends.
            const float sEps = 1e-5f;
            return t >= 0f && s >= -sEps && s <= 1f + sEps;
        }

        /// <summary>Unit direction at relative angle <paramref name="ang"/> (uHat rotated CCW by ang).</summary>
        static Vector2 Dir(Vector2 uHat, float ang)
        {
            float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
            return new Vector2(uHat.x * ca - uHat.y * sa, uHat.x * sa + uHat.y * ca);
        }

        /// <summary>Signed angle (radians) from <paramref name="uHat"/> to <paramref name="v"/>, in (-π, π].</summary>
        static float RelAngle(Vector2 uHat, Vector2 v) =>
            Mathf.Atan2(uHat.x * v.y - uHat.y * v.x, uHat.x * v.x + uHat.y * v.y);
    }
}
