using Enums;

public static class DirectionConverter
{
    public static (int X, int Y) Convert(ChessPieceDirection direction)
    {
        switch (direction)
        {
            case ChessPieceDirection.Up:
                return (0, 1);
            case ChessPieceDirection.UpperRight:
                return (1, 1);
            case ChessPieceDirection.Right:
                return (1, 0);
            case ChessPieceDirection.LowerRight:
                return (1, -1);
            case ChessPieceDirection.Down:
                return (0, -1);
            case ChessPieceDirection.LowerLeft:
                return (-1, -1);
            case ChessPieceDirection.Left:
                return (-1, 0);
            case ChessPieceDirection.UpperLeft:
                return (-1, 1);
            default:
                return (0, 0);
        }
    }

}
