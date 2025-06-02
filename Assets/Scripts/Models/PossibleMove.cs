using UnityEngine;

public class PossibleMove
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsAttack { get; set; } = false;

    public PossibleMove(int x, int y, bool isAttack = false)
    {
        X = x;
        Y = y;
        IsAttack = isAttack;
    }
}
