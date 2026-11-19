using System.Collections.Generic;
using UnityEngine;

namespace Tsakel.Game
{
    public enum PhotoVerdict { Valid, NoSubject, TooFar, OutOfFrame, TooSmall, Blocked }

    public struct PhotoEvaluation
    {
        public PhotoVerdict Verdict;
        public AnimalSubject Animal;
        public float Distance;
        public float Coverage;
        public bool IsValid { get { return Verdict == PhotoVerdict.Valid; } }
    }

    /// <summary>Reglas del GDD para una foto valida: distancia, % del encuadre y sin obstaculos.</summary>
    public static class PhotoEvaluator
    {
        /// <summary>Rectangulo del visor en coordenadas de viewport (cuadrado centrado).</summary>
        public static Rect ViewfinderRect(Camera cam, float heightFraction)
        {
            float aspect = (float)cam.pixelWidth / Mathf.Max(1, cam.pixelHeight);
            float w = heightFraction / aspect;
            return new Rect(0.5f - w * 0.5f, 0.5f - heightFraction * 0.5f, w, heightFraction);
        }

        /// <summary>Animal visible cuyo centro esta mas cerca de la mira, dentro del visor.</summary>
        public static AnimalSubject Pick(Camera cam, IList<AnimalSubject> animals, float heightFraction)
        {
            if (animals == null) return null;
            Rect vf = ViewfinderRect(cam, heightFraction);
            AnimalSubject best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < animals.Count; i++)
            {
                AnimalSubject a = animals[i];
                Bounds b;
                if (a == null || !a.IsPhotographable || !a.TryGetBounds(out b)) continue;
                Vector3 v = cam.WorldToViewportPoint(b.center);
                if (v.z <= 0f || !vf.Contains(new Vector2(v.x, v.y))) continue;
                float d = new Vector2(v.x - 0.5f, v.y - 0.5f).sqrMagnitude;
                if (d < bestD) { bestD = d; best = a; }
            }
            return best;
        }

        public static PhotoEvaluation Evaluate(Camera cam, AnimalSubject animal, float heightFraction, float maxDistance, float minCoverage, float minVisibleFraction)
        {
            PhotoEvaluation e = new PhotoEvaluation();
            e.Animal = animal;
            e.Verdict = PhotoVerdict.NoSubject;
            Bounds b;
            if (animal == null || !animal.IsPhotographable || !animal.TryGetBounds(out b)) return e;

            Vector3 camPos = cam.transform.position;
            Rect vf = ViewfinderRect(cam, heightFraction);
            e.Distance = Vector3.Distance(camPos, b.ClosestPoint(camPos));

            // proyeccion de las 8 esquinas de la caja del animal
            Vector3 c = b.center;
            Vector3 ex = b.extents;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            int front = 0;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = c + new Vector3((i & 1) == 0 ? -ex.x : ex.x, (i & 2) == 0 ? -ex.y : ex.y, (i & 4) == 0 ? -ex.z : ex.z);
                Vector3 v = cam.WorldToViewportPoint(corner);
                if (v.z <= 0f) continue;
                front++;
                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }
            Vector3 cv = cam.WorldToViewportPoint(c);
            if (front == 0 || cv.z <= 0f || !vf.Contains(new Vector2(cv.x, cv.y)))
            {
                e.Verdict = PhotoVerdict.OutOfFrame;
                return e;
            }

            if (e.Distance > maxDistance) { e.Verdict = PhotoVerdict.TooFar; return e; }

            float ix = Mathf.Max(0f, Mathf.Min(maxX, vf.xMax) - Mathf.Max(minX, vf.xMin));
            float iy = Mathf.Max(0f, Mathf.Min(maxY, vf.yMax) - Mathf.Max(minY, vf.yMin));
            e.Coverage = (ix * iy) / (vf.width * vf.height);
            if (e.Coverage < minCoverage) { e.Verdict = PhotoVerdict.TooSmall; return e; }

            // obstaculos: 9 puntos del animal (centro + 8 esquinas acercadas al centro)
            int visible = 0;
            int total = 9;
            for (int i = 0; i < total; i++)
            {
                Vector3 p = c;
                if (i > 0)
                {
                    int k = i - 1;
                    Vector3 corner = c + new Vector3((k & 1) == 0 ? -ex.x : ex.x, (k & 2) == 0 ? -ex.y : ex.y, (k & 4) == 0 ? -ex.z : ex.z);
                    p = Vector3.Lerp(c, corner, 0.6f);
                }
                p.y = Mathf.Max(p.y, 0.1f); // evita puntos bajo el piso
                Vector3 dir = p - camPos;
                float len = dir.magnitude;
                if (len < 0.5f) { visible++; continue; }
                Vector3 start = camPos + dir / len * 0.4f; // sale de la capsula del jugador
                if (!Physics.Linecast(start, p, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) visible++;
            }
            if ((float)visible / total < minVisibleFraction) { e.Verdict = PhotoVerdict.Blocked; return e; }

            e.Verdict = PhotoVerdict.Valid;
            return e;
        }
    }
}
