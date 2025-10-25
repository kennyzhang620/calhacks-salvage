using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntegrationChunk : MonoBehaviour
{
    public Mesh mesh;
    public float RectSize = 1.0f; // Size in m
    public float L, W = 0;
    private void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    float _t = 6.0f;
    bool _inA = false;
    IEnumerator UpdateMeasures()
    {
        _t -= Time.deltaTime;
        if (_t < 5)
        {

            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
            {
                Debug.LogError("No mesh found.");
                yield break;
            }

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Step 1: Find boundary edges (edges used only once)
            Dictionary<(int, int), int> edgeCount = new Dictionary<(int, int), int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int[] tri = { triangles[i], triangles[i + 1], triangles[i + 2] };

                for (int j = 0; j < 3; j++)
                {
                    int v1 = tri[j];
                    int v2 = tri[(j + 1) % 3];

                    var edge = (Mathf.Min(v1, v2), Mathf.Max(v1, v2));

                    if (edgeCount.ContainsKey(edge))
                        edgeCount[edge]++;
                    else
                        edgeCount[edge] = 1;
                }
            }

            List<(int, int)> boundaryEdges = edgeCount
                .Where(e => e.Value == 1)
                .Select(e => e.Key)
                .ToList();

            // Step 2: Build adjacency map
            Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();
            foreach (var edge in boundaryEdges)
            {
                if (!adjacency.ContainsKey(edge.Item1))
                    adjacency[edge.Item1] = new List<int>();
                if (!adjacency.ContainsKey(edge.Item2))
                    adjacency[edge.Item2] = new List<int>();

                adjacency[edge.Item1].Add(edge.Item2);
                adjacency[edge.Item2].Add(edge.Item1);
            }

            // Step 3: Trace all edge loops (to support holes)
            List<List<int>> allLoops = new List<List<int>>();
            HashSet<(int, int)> visitedEdges = new HashSet<(int, int)>();

            foreach (var start in adjacency.Keys)
            {
                foreach (var neighbor in adjacency[start])
                {
                    var edge = (Mathf.Min(start, neighbor), Mathf.Max(start, neighbor));
                    if (visitedEdges.Contains(edge)) continue;

                    List<int> loop = new List<int>();
                    int current = start;
                    int previous = -1;

                    while (true)
                    {
                        loop.Add(current);

                        int next = adjacency[current].FirstOrDefault(n =>
                            n != previous &&
                            !visitedEdges.Contains((Mathf.Min(current, n), Mathf.Max(current, n))));

                        if (next == 0 && current != start) break;

                        visitedEdges.Add((Mathf.Min(current, next), Mathf.Max(current, next)));

                        previous = current;
                        current = next;

                        if (current == start)
                        {
                            loop.Add(current); // Close the loop
                            break;
                        }

                        if (next == -1) break; // Dead end
                    }

                    if (loop.Count > 2)
                        allLoops.Add(loop);
                }
            }

            if (allLoops.Count == 0)
            {
                Debug.LogWarning("No perimeter loops found.");
                yield break;
            }

            // Step 4: Calculate total perimeter and area
            float totalPerimeter = 0f;
            float totalArea = 0f;

            foreach (var loop in allLoops)
            {
                List<Vector3> worldPoints = loop
                    .Select(i => transform.TransformPoint(vertices[i]))
                    .ToList();

                totalPerimeter += CalculatePerimeter(worldPoints);
                totalArea += CalculateShoelaceAreaXZ(worldPoints);
            }

            // Output
            Debug.Log($"Total Perimeter: {totalPerimeter:F3} meters");
            Debug.Log($"Projected Surface Area (XZ): {totalArea:F3} m²");

            // Optionally: Calculate L x H from all loop points
            Vector3[] allBoundaryPoints = allLoops
                .SelectMany(loop => loop)
                .Distinct()
                .Select(i => transform.TransformPoint(vertices[i]))
                .ToArray();

            if (allBoundaryPoints.Length > 0)
            {
                float minX = allBoundaryPoints.Min(p => p.x);
                float maxX = allBoundaryPoints.Max(p => p.x);
                float minZ = allBoundaryPoints.Min(p => p.z);
                float maxZ = allBoundaryPoints.Max(p => p.z);

                float length = maxX - minX;
                float height = maxZ - minZ;

                Debug.Log($"Approximate L x H (XZ projection): {length:F3}m × {height:F3}m");
                L = length * 3.28084f;
                W = height * 3.28084f;

                name = ((float)System.Math.Round(minX, 6)).ToString() + ((float)System.Math.Round(maxZ, 6)).ToString() + Mathf.Floor(Camera.main.transform.rotation.eulerAngles.y / 90).ToString();

            }

            RectSize = totalArea * 10.7639f;

            if (!Settings.pieces.ContainsKey(name))
            {
                Settings.pieces.Add(name, RectSize);
            }
            else
            {
                Settings.pieces[name] = RectSize;
                _inA = true;
            }

            if (!Settings.piecesX.ContainsKey(name))
            {
                Settings.piecesX.Add(name, L);
            }
            else
            {
                Settings.piecesX[name] = L;
            }

            if (!Settings.piecesY.ContainsKey(name))
            {
                Settings.piecesY.Add(name, W);
            }
            else
            {
                Settings.piecesY[name] = W;
            }
        }

        if (_t < 3f && _inA)
        {
            Destroy(gameObject);
        }
        yield return null;

    }

    float CalculatePerimeter(List<Vector3> points)
    {
        float p = 0f;
        for (int i = 0; i < points.Count - 1; i++) // last point is duplicate of first
            p += Vector3.Distance(points[i], points[i + 1]);
        return p;
    }

    float CalculateShoelaceAreaXZ(List<Vector3> points)
    {
        float area = 0f;
        for (int i = 0; i < points.Count - 1; i++) // last point is duplicate of first
        {
            Vector2 a = new Vector2(points[i].x, points[i].z);
            Vector2 b = new Vector2(points[i + 1].x, points[i + 1].z);
            area += (a.x * b.y) - (b.x * a.y);
        }
        return Mathf.Abs(area * 0.5f);
    }

    private void Update()
    {
        StartCoroutine(UpdateMeasures());
    }

    private void OnDestroy()
    {

            if (Settings.pieces.ContainsKey(name))
            {
                Settings.pieces[name] = 0;
                Settings.pieces.Remove(name);
            }
            if (Settings.piecesX.ContainsKey(name))
            {
                Settings.piecesX[name] = 0;
                Settings.piecesX.Remove(name);
            }
            if (Settings.piecesY.ContainsKey(name))
            {
                Settings.piecesY[name] = 0;
                Settings.piecesY.Remove(name);
            }
    }
}

