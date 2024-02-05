using UnityEngine;
using System;

public static class ExtensionMethods
{
    public static bool ColorEquals(this Color32 a, Color32 b, int threshold)
    {
        return Math.Abs(a.r - b.r) <= threshold && Math.Abs(a.g - b.g) <= threshold && Math.Abs(a.b - b.b) <= threshold && Math.Abs(a.a - b.a) <= threshold;
    }
    public static float SqrDistanceToLineSegment(this Vector2Int point, Vector2Int x, Vector2Int y)
    {
        int px = y.x - x.x;
        int py = y.y - x.y;
        int norm = px * px + py * py;
        float u = ((point.x - x.x) * px + (point.y - x.y) * py) / (float)norm;
        u = Mathf.Clamp01(u);
        float a = x.x + u * px;
        float b = x.y + u * py;
        float da = a - point.x;
        float db = b - point.y;
        return (da * da + db * db);
    }
    public static int SqrDistanceToPoint(this Vector2Int point, Vector2Int a)
    {
        int x = point.x - a.x;
        int y = point.y - a.y;
        return x * x + y * y;
    }
}
