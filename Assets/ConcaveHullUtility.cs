using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ConcaveHullUtility
{
    public static List<Vector2> ComputeConcaveHull(List<Vector2> inputPoints, float concavity = 0.5f, float scaleFactor = 1.5f)
    {

        var points = inputPoints.Distinct().ToList();

        if (points.Count <= 0) return points;

        // Start from leftmost point
        Vector2 start = points.OrderBy(p => p.x).ThenBy(p => p.y).First();
        List<Vector2> hull = new List<Vector2> { start };

        Vector2 current = start;
        Vector2 prevDir = Vector2.right;

        while (true)
        {
            Vector2? nextPoint = null;
            float bestAngle = float.MaxValue;
            float searchRadius = EstimateSearchRadius(points, scaleFactor);

            foreach (var candidate in points)
            {
                if (candidate == current)
                    continue;

                float dist = Vector2.Distance(candidate, current);
                if (dist > searchRadius)
                    continue;

                Vector2 dir = (candidate - current).normalized;
                float angle = Vector2.SignedAngle(prevDir, dir);
                angle = angle < 0 ? angle + 360 : angle;

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    nextPoint = candidate;
                }
            }

            if (nextPoint == null || nextPoint == start)
                break;

            hull.Add(nextPoint.Value);
            prevDir = (nextPoint.Value - current).normalized;
            current = nextPoint.Value;

            if (hull.Count > 1000) break; // prevent infinite loops
        }

        return hull;
    }

    static float EstimateSearchRadius(List<Vector2> points, float scaleFactor)
    {
        float avg = 0f;
        int count = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                avg += Vector2.Distance(points[i], points[j]);
                count++;
            }
        }
        return (avg / count) * scaleFactor;
    }

    public static List<Vector2> ComputeConvexHull(Vector2[] points)
    {

        var sorted = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();

        List<Vector2> lower = new List<Vector2>();
        foreach (var p in sorted)
        {
            while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        List<Vector2> upper = new List<Vector2>();
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            var p = sorted[i];
            while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        if (lower.Count - 1 >= 0)
            lower.RemoveAt(lower.Count - 1);

        if (upper.Count - 1 >= 0)
            upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);

        return lower;
    }


    static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }


    public static List<Vector2> TraceContour(Vector2[] points, float cellSize = 0.1f)
    {
        if (points.Length < 3) return new List<Vector2>(points);

        // 1. Compute bounds
        var bounds = GetBounds(points);
        int width = Mathf.CeilToInt(bounds.size.x / cellSize);
        int height = Mathf.CeilToInt(bounds.size.y / cellSize);
        bool[,] grid = new bool[width + 1, height + 1];

        // 2. Rasterize points to grid
        foreach (var p in points)
        {
            int x = Mathf.FloorToInt((p.x - bounds.min.x) / cellSize);
            int y = Mathf.FloorToInt((p.y - bounds.min.y) / cellSize);
            if (x >= 0 && x <= width && y >= 0 && y <= height)
                grid[x, y] = true;
        }

        // 3. Apply Marching Squares
        return MarchingSquares(grid, cellSize, bounds.min);
    }

    private static List<Vector2> MarchingSquares(bool[,] grid, float cellSize, Vector2 offset)
    {
        List<Vector2> contour = new List<Vector2>();
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        // Simple marching squares loop (4-neighbor tracing)
        for (int y = 0; y < h - 1; y++)
        {
            for (int x = 0; x < w - 1; x++)
            {
                int state = 0;
                if (grid[x, y]) state |= 1;
                if (grid[x + 1, y]) state |= 2;
                if (grid[x + 1, y + 1]) state |= 4;
                if (grid[x, y + 1]) state |= 8;

                Vector2 basePos = offset + new Vector2(x, y) * cellSize;

                // Add contour edges (simplified case; handles only 4 corners cleanly)
                switch (state)
                {
                    case 1:
                    case 14:
                        contour.Add(basePos + new Vector2(0, 0.5f) * cellSize);
                        contour.Add(basePos + new Vector2(0.5f, 0) * cellSize);
                        break;
                    case 2:
                    case 13:
                        contour.Add(basePos + new Vector2(0.5f, 0) * cellSize);
                        contour.Add(basePos + new Vector2(1, 0.5f) * cellSize);
                        break;
                    case 3:
                    case 12:
                        contour.Add(basePos + new Vector2(0, 0.5f) * cellSize);
                        contour.Add(basePos + new Vector2(1, 0.5f) * cellSize);
                        break;
                    case 4:
                    case 11:
                        contour.Add(basePos + new Vector2(1, 0.5f) * cellSize);
                        contour.Add(basePos + new Vector2(0.5f, 1) * cellSize);
                        break;
                    case 6:
                    case 9:
                        contour.Add(basePos + new Vector2(0.5f, 0) * cellSize);
                        contour.Add(basePos + new Vector2(0.5f, 1) * cellSize);
                        break;
                    case 7:
                    case 8:
                        contour.Add(basePos + new Vector2(0, 0.5f) * cellSize);
                        contour.Add(basePos + new Vector2(0.5f, 1) * cellSize);
                        break;
                    case 10:
                        contour.Add(basePos + new Vector2(0, 0.5f) * cellSize);
                        contour.Add(basePos + new Vector2(1, 0.5f) * cellSize);
                        break;
                    default:
                        break;
                }
            }
        }

        return contour;
    }

    private static Bounds GetBounds(Vector2[] points)
    {
        Vector2 min = points[0], max = points[0];
        foreach (var p in points)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        return new Bounds((min + max) * 0.5f, max - min);
    }

}
