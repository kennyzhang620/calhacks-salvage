using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteAlways]
public class BVisualizer : MonoBehaviour
{
    public Color pointColor = Color.red;
    public Color lineColor = Color.yellow;
    public float pointSize = 0.1f;
    public bool showLabels = true;
    public bool closeLoop = true;

    private void OnDrawGizmos()
    {
        Vector3[] points3D = Settings.AllBoundaryPoints;
        if (points3D == null || points3D.Length < 2)
            return;

        // Convert to XZ 2D projection
        Vector2[] points2D = points3D.Select(p => new Vector2(p.x, p.z)).ToArray();

        // Draw points
        Gizmos.color = pointColor;
        foreach (Vector2 point in points2D)
        {
            Vector3 pos = new Vector3(point.x, 0f, point.y); // Y = 0 for 2D projection
            Gizmos.DrawSphere(pos, pointSize);
        }

        // Draw lines
        Gizmos.color = lineColor;
        for (int i = 0; i < points2D.Length - 1; i++)
        {
            Vector3 p1 = new Vector3(points2D[i].x, 0f, points2D[i].y);
            Vector3 p2 = new Vector3(points2D[i + 1].x, 0f, points2D[i + 1].y);
            Gizmos.DrawLine(p1, p2);
        }

        // Optional: close the loop
        if (closeLoop && points2D.Length > 2)
        {
            Vector3 pStart = new Vector3(points2D[0].x, 0f, points2D[0].y);
            Vector3 pEnd = new Vector3(points2D[points2D.Length - 1].x, 0f, points2D[points2D.Length - 1].y);
            Gizmos.DrawLine(pEnd, pStart);
        }

#if UNITY_EDITOR
        if (showLabels)
        {
            // Draw label at center
            Vector2 center2D = Vector2.zero;
            foreach (var p in points2D) center2D += p;
            center2D /= points2D.Length;

            Vector3 labelPos = new Vector3(center2D.x, 0.1f, center2D.y);
            GUIStyle style = new GUIStyle()
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState { textColor = Color.white }
            };

            Handles.Label(labelPos, $"Area: {Settings.Area:F2} ft²\nSize: {Settings.Length:F2} ft × {Settings.Width:F2} ft", style);
        }
#endif
    }
}
