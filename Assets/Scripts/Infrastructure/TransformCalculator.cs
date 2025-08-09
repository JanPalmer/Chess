using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

namespace Infrastructure
{
    public static class TransformCalculator
    {
        public static (float X, float Y) CalculateTransform(float x, float y)
        {
            x *= 0.66f;
            y *= 0.66f;
            x += -2.3f;
            y += -2.3f;

            return (x, y);
        }

        public static Quaternion CalculateRotation(Vector2 start, Vector2 end)
        {
            float angle = Vector2.SignedAngle(start, end);
            return Quaternion.Euler(0, 0, angle);
        }
    }
}