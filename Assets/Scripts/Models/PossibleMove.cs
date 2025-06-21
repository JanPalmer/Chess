using UnityEngine;

public class PossibleMove
{
    public Chessman ChessPiece;
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsAttack { get; set; } = false;

    public PossibleMove(Chessman chessPiece, int x, int y, bool isAttack = false)
    {
        ChessPiece = chessPiece;
        X = x;
        Y = y;
        IsAttack = isAttack;
    }
}
