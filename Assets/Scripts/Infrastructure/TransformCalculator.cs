using UnityEngine;

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
}
