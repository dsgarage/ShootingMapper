using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Runtime.Simulation
{
    /// <summary>
    /// Editor/Runtime共用の弾道シミュレーションロジック。
    /// EditorApplication.update から呼び出してPlayモード不要のプレビューを実現する。
    /// </summary>
    public static class BulletSimulator
    {
        public struct BulletState
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Lifetime;
            public bool Active;
        }

        public static BulletState[] CreateBullets(ShmupBulletPatternData pattern, Vector2 origin, float aimAngle)
        {
            if (pattern == null || pattern.bulletCount <= 0)
                return System.Array.Empty<BulletState>();

            var bullets = new BulletState[pattern.bulletCount];
            float halfSpread = pattern.spreadAngle * 0.5f;
            float step = pattern.bulletCount > 1
                ? pattern.spreadAngle / (pattern.bulletCount - 1)
                : 0f;

            for (int i = 0; i < pattern.bulletCount; i++)
            {
                float angle;
                switch (pattern.spreadType)
                {
                    case BulletSpreadType.Circle:
                        angle = aimAngle + (360f / pattern.bulletCount) * i + pattern.angleOffset;
                        break;
                    case BulletSpreadType.Fan:
                        angle = aimAngle - halfSpread + step * i + pattern.angleOffset;
                        break;
                    case BulletSpreadType.Line:
                        angle = aimAngle + pattern.angleOffset;
                        break;
                    default: // Random
                        angle = aimAngle + Random.Range(-halfSpread, halfSpread) + pattern.angleOffset;
                        break;
                }

                float rad = angle * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                bullets[i] = new BulletState
                {
                    Position = origin,
                    Velocity = dir * pattern.speed,
                    Lifetime = 0f,
                    Active = true
                };
            }

            return bullets;
        }

        public static void StepSimulation(ref BulletState bullet, float deltaTime, float acceleration)
        {
            if (!bullet.Active) return;

            bullet.Velocity += bullet.Velocity.normalized * acceleration * deltaTime;
            bullet.Position += bullet.Velocity * deltaTime;
            bullet.Lifetime += deltaTime;
        }
    }
}
