using UnityEngine;

namespace ShmupCreator.Runtime.Simulation
{
    /// <summary>
    /// ウェイポイントベースのパス評価ロジック。
    /// 折れ線パスおよびベジェ曲線パスの位置計算を提供する。
    /// </summary>
    public static class PathEvaluator
    {
        public static Vector2 EvaluateLinearPath(Vector2[] waypoints, float t)
        {
            if (waypoints == null || waypoints.Length == 0)
                return Vector2.zero;
            if (waypoints.Length == 1)
                return waypoints[0];

            t = Mathf.Clamp01(t);
            float totalSegments = waypoints.Length - 1;
            float segmentT = t * totalSegments;
            int segmentIndex = Mathf.Min((int)segmentT, waypoints.Length - 2);
            float localT = segmentT - segmentIndex;

            return Vector2.Lerp(waypoints[segmentIndex], waypoints[segmentIndex + 1], localT);
        }

        public static Vector2 EvaluateBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return u * u * u * p0
                 + 3f * u * u * t * p1
                 + 3f * u * t * t * p2
                 + t * t * t * p3;
        }
    }
}
