using System.Collections.Generic;
using Enums;
using Models;

public class DirectionArrow
{
    public UnitDirection Direction { get; set; } = UnitDirection.Right;

    public PossibleMove Move { get; set; }

    // Depth of movement - how many single moves the unit must do to reach the End position
    public int Depth { get; set; } = 1;

    public override bool Equals(object obj)
    {
        if (obj is not DirectionArrow)
        {
            return false;
        }

        var arrowToCheck = obj as DirectionArrow;

        return this.Move.Equals(arrowToCheck.Move)
            && this.Direction == arrowToCheck.Direction
            && this.Depth == arrowToCheck.Depth;
    }

    public DirectionArrow() { }

    public DirectionArrow(DirectionArrow toCopy)
    {
        Direction = toCopy.Direction;
        Depth = toCopy.Depth;
        Move = new PossibleMove(toCopy.Move);
    }

    public DirectionArrow(
        UnitDirection direction,
        int depth,
        Chessman chessPiece,
        int xStart,
        int yStart,
        int xEnd,
        int yEnd,
        IEnumerable<DirectionArrow> directions,
        List<(IChessPiece PossibleTarget, UnitVisibility Visibility)> targets,
        Chessman attackedChessPiece = null,
        int attackedChessPieceHealthLost = 0)
    {
        Direction = direction;
        Depth = depth;
        Move = new PossibleMove(chessPiece, xStart, yStart, xEnd, yEnd, directions, targets, attackedChessPiece, attackedChessPieceHealthLost);
    }
}
