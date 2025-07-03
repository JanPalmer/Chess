using Enums;

public static class DirectionConverter
{
    public static (int X, int Y) Convert(UnitDirection direction)
    {
        switch (direction)
        {
            case UnitDirection.Up:
                return (0, 1);
            case UnitDirection.UpperRight:
                return (1, 1);
            case UnitDirection.Right:
                return (1, 0);
            case UnitDirection.LowerRight:
                return (1, -1);
            case UnitDirection.Down:
                return (0, -1);
            case UnitDirection.LowerLeft:
                return (-1, -1);
            case UnitDirection.Left:
                return (-1, 0);
            case UnitDirection.UpperLeft:
                return (-1, 1);
            default:
                return (0, 0);
        }
    }

}
