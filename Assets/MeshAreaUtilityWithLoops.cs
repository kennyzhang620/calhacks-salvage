using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshAreaUtilityWithLoops
{
    public class LoopData
    {
        public List<Vector3> worldPoints;
        public float projectedArea;
    }

    public static float CalculateProjectedOuterAreaXZ(Mesh mesh, Transform transform, out List<LoopData> allLoops)
    {
        allLoops = new List<LoopData>();

        if (mesh == null || transform == null)
        {
            Debug.LogWarning("Mesh or Transform is null.");
            return 0f;
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Step 1: Find boundary edges
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

        // Step 2: Get boundary edges (used only once)
        var boundaryEdges = edgeCount.Where(e => e.Value == 1).Select(e => e.Key).ToList();

        // Step 3: Build adjacency
        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();
        foreach (var edge in boundaryEdges)
        {
            if (!adjacency.ContainsKey(edge.Item1)) adjacency[edge.Item1] = new List<int>();
            if (!adjacency.ContainsKey(edge.Item2)) adjacency[edge.Item2] = new List<int>();
            adjacency[edge.Item1].Add(edge.Item2);
            adjacency[edge.Item2].Add(edge.Item1);
        }

        // Step 4: Trace all loops
        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        foreach (var start in adjacency.Keys)
        {
            foreach (var neighbor in adjacency[start])
            {
                var edgeKey = (Mathf.Min(start, neighbor), Mathf.Max(start, neighbor));
                if (visited.Contains(edgeKey)) continue;

                List<int> loop = new List<int>();
                int current = start;
                int previous = -1;

                while (true)
                {
                    loop.Add(current);
                    int next = adjacency[current].FirstOrDefault(n =>
                        n != previous &&
                        !visited.Contains((Mathf.Min(current, n), Mathf.Max(current, n))));

                    if (next == 0 && current != start)
                        break;

                    visited.Add((Mathf.Min(current, next), Mathf.Max(current, next)));
                    previous = current;
                    current = next;

                    if (current == start)
                    {
                        loop.Add(current); // close
                        break;
                    }

                    if (next == -1) break;
                }

                if (loop.Count >= 3)
                {
                    // Convert to world space
                    var worldPoints = loop.Select(i => transform.TransformPoint(vertices[i])).ToList();

                    float area = CalculateShoelaceXZ(worldPoints);

                    allLoops.Add(new LoopData
                    {
                        worldPoints = worldPoints,
                        projectedArea = area
                    });
                }
            }
        }

        if (allLoops.Count == 0)
        {
            Debug.LogWarning("No boundary loops found.");
            return 0f;
        }

        // Step 5: Choose the loop with the largest projected area
        var outerLoop = allLoops.OrderByDescending(l => l.projectedArea).First();
        return outerLoop.projectedArea;
    }

    public static float CalculateShoelaceXZ(List<Vector3> points)
    {
        float area = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 a = new Vector2(points[i].x, points[i].z);
            Vector2 b = new Vector2(points[i + 1].x, points[i + 1].z);
            area += (a.x * b.y) - (b.x * a.y);
        }
        return Mathf.Abs(area * 0.5f);
    }
}
