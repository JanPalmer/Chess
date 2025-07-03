using Enums;
using Models;

public class DirectionArrow
{
    public UnitDirection Direction { get; set; } = UnitDirection.Right;

    public PossibleMove Move { get; set; }

    // Depth of movement - how many single moves the unit must do to reach the End position
    public int Depth { get; set; } = 1;
}
