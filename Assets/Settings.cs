using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Settings
{
    public static float Area = 0.0f;
    public static float Length = 0.0f;
    public static float Width = 0.0f;
    public static Mesh cmesh;
    public static int MeshCount = 1024;
    public static bool Freeze = false;
    static GameObject b = new GameObject();
    public static Vector3[] AllBoundaryPoints;

    public static string SerializeData()
    {
        return "CArea," + (Area + Length * Width) / 2 + ",MArea," + Area + ",Length," + Length + ",Width," + Width;
    }

    public static void Clear()
    {
        cmesh.Clear();
        Area = 0; Length = 0; Width = 0;
        MeshCount = 1024;
    }

    static float CalculatePerimeter(List<Vector3> points)
    {
        float p = 0f;
        for (int i = 0; i < points.Count - 1; i++) // last point is duplicate of first
            p += Vector3.Distance(points[i], points[i + 1]);
        return p;
    }

    static float CalculateShoelaceAreaXZ(List<Vector3> points)
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

    public static void CalculateMesh2()
    {

        Mesh mesh = cmesh;

        if (!mesh) return;

        Vector3[] verts = mesh.vertices;

        // 1. Project to XZ
        Vector2[] projected = ProjectToXZ(verts);

        List<Vector2> hull = ConcaveHullUtility.TraceContour(projected, 0.2f);
        List<Vector2> hull2 = ConcaveHullUtility.ComputeConvexHull(projected);

        AllBoundaryPoints = hull.Select(p => new Vector3(p.x, 1, p.y)).ToArray();

        // 3. Area
        float area = (ComputePolygonArea(hull) + ComputePolygonArea(hull2))/2;

        // 4. Length and Width via Oriented Bounding Box (OBB)
        (float length, float width) = ComputeLengthWidthOBB(hull2);

        Debug.Log($"Projected XZ Area: {area:F2}, Length: {length:F2}, Width: {width:F2}");
        Area = area * 10.7639f;
        Length = length * 3.28084f;
        Width = width * 3.28084f;
    }

    static List<Vector2> Downsample(List<Vector2> points, int maxPoints = 200)
    {
        if (points.Count <= maxPoints) return points;

        float step = (float)points.Count / maxPoints;
        var sampled = new List<Vector2>();
        for (int i = 0; i < points.Count; i += Mathf.CeilToInt(step))
            sampled.Add(points[i]);
        return sampled;
    }


    static Vector2[] ProjectToXZ(Vector3[] vertices)
    {
        Vector2[] projected = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            projected[i] = new Vector2(vertices[i].x, vertices[i].z); // XZ projection
        }
        return projected;
    }

    static float ComputePolygonArea(List<Vector2> polygon)
    {
        float area = 0f;
        int n = polygon.Count;
        for (int i = 0; i < n; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % n];
            area += (current.x * next.y) - (next.x * current.y);
        }
        return Mathf.Abs(area) * 0.5f;
    }

    static (float length, float width) ComputeLengthWidthOBB(List<Vector2> points)
    {
        float minArea = float.MaxValue;
        float bestLength = 0f, bestWidth = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];

            Vector2 edge = (p2 - p1).normalized;

            Vector2 axisX = edge;
            Vector2 axisY = new Vector2(-edge.y, edge.x);  // perpendicular

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (var p in points)
            {
                float xProj = Vector2.Dot(p, axisX);
                float yProj = Vector2.Dot(p, axisY);

                minX = Mathf.Min(minX, xProj);
                maxX = Mathf.Max(maxX, xProj);
                minY = Mathf.Min(minY, yProj);
                maxY = Mathf.Max(maxY, yProj);
            }

            float length = maxX - minX;
            float width = maxY - minY;
            float area = length * width;

            if (area < minArea)
            {
                minArea = area;
                bestLength = Mathf.Max(length, width);
                bestWidth = Mathf.Min(length, width);
            }
        }

        return (bestLength, bestWidth);
    }





    private static bool IsClockwiseXZ(List<Vector3> points)
    {
        float sum = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];
            sum += (b.x - a.x) * (b.z + a.z); // Shoelace-style winding check
        }
        return sum > 0f; // Clockwise if positive
    }

    public static void CalculateMesh()
    {
        Mesh mesh = cmesh;
        Transform transform = b.transform;
            if (mesh == null)
            {
                Debug.Log("No mesh found.");
                return;
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
            }

            // Step 4: Calculate total perimeter and area
            float totalPerimeter = 0f;
            float totalArea = 0f;

        foreach (var loop in allLoops)
        {
            List<Vector3> worldPoints = loop
                .Select(i => transform.TransformPoint(vertices[i]))
                .ToList();

            float area = CalculateShoelaceAreaXZ(worldPoints);

            if (IsClockwiseXZ(worldPoints))
            {
                totalArea += area; // outer loop
            }
            else
            {
                totalArea -= area; // hole
            }

            totalPerimeter += CalculatePerimeter(worldPoints); // Always add perimeter
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
                Length = length * 3.28084f;
                Width = height * 3.28084f;

            }

            AllBoundaryPoints = allBoundaryPoints;
            Area = totalArea * 10.7639f;

        }

}
