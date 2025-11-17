using Enums;
using Models;
using UnityEngine;

public static class TestHelpers
{
    public static Board InitializeBoard_1v3()
    {
        var resultBoard = new Board();

        InitializePiece(resultBoard, UnitRole.Rook, PlayerSide.Orange, 1, 6);

        InitializePiece(resultBoard, UnitRole.Rook, PlayerSide.Blue, 14, 6);
        InitializePiece(resultBoard, UnitRole.Pawn, PlayerSide.Blue, 14, 4);
        InitializePiece(resultBoard, UnitRole.Pawn, PlayerSide.Blue, 14, 2);

        InitializeWalls(resultBoard);

        return resultBoard;
    }

    public static Board InitializeBoard_3v3()
    {
        var resultBoard = new Board();

        InitializePiece(resultBoard, UnitRole.Rook, PlayerSide.Orange, 1, 6);
        InitializePiece(resultBoard, UnitRole.Pawn, PlayerSide.Orange, 1, 4);
        InitializePiece(resultBoard, UnitRole.Pawn, PlayerSide.Orange, 1, 2);

        InitializePiece(resultBoard, UnitRole.Rook, PlayerSide.Blue, 14, 6);
        InitializePiece(resultBoard, UnitRole.Pawn, PlayerSide.Blue, 14, 4);
        InitializePiece(resultBoard, UnitRole.Pawn, PlayerSide.Blue, 14, 2);

        InitializeWalls(resultBoard);

        return resultBoard;
    }

    private static void InitializeWalls(Board board)
    {
        InitializeWall(board, 4, 2); InitializeWall(board, 4, 1); InitializeWall(board, 5, 1);
        InitializeWall(board, 3, 8); InitializeWall(board, 3, 7); InitializeWall(board, 4, 8);
        InitializeWall(board, 10, 8); InitializeWall(board, 11, 8); InitializeWall(board, 11, 7);
        InitializeWall(board, 11, 1); InitializeWall(board, 12, 1); InitializeWall(board, 12, 2);

        InitializeWall(board, 7, 7); InitializeWall(board, 8, 7); InitializeWall(board, 6, 6); InitializeWall(board, 7, 6);
        InitializeWall(board, 5, 5); InitializeWall(board, 6, 5); InitializeWall(board, 6, 4);

        InitializeWall(board, 9, 5); InitializeWall(board, 9, 4); InitializeWall(board, 10, 4); InitializeWall(board, 8, 3);
        InitializeWall(board, 9, 3); InitializeWall(board, 7, 2); InitializeWall(board, 8, 2);
    }

    public static void PerformMove(Board board, DirectionArrow arrow)
    {
        if (arrow.Move.AttackedChessPiece != null)
        {
            board.PerformAttack(arrow.Move, arrow.Move.AttackedChessPiece);
        }

        board.MoveChessPiece(arrow.Move, arrow.Direction);
    }

    #region Element initialization
    private static Chessman InitializePiece(Board board, UnitRole role, PlayerSide player, int x, int y)
    {
        var piece = new Chessman()
        {
            Name = role.ToString(),
            XBoard = x,
            YBoard = y,
            Player = player,
            Role = role,
            Board = board,
            Direction = (player == PlayerSide.Orange) ? UnitDirection.Right : UnitDirection.Left,
        };

        piece.SetPieceStats();

        board.SetPosition(piece, x, y);

        return piece;
    }

    private static Wall InitializeWall(Board board, int x, int y)
    {
        var wall = new Wall()
        {
            XBoard = x,
            YBoard = y,
            Board = board,
            Role = UnitRole.Wall,
            Player = PlayerSide.NPC,
        };

        board.SetPosition(wall, x, y);

        return wall;
    }
    #endregion
}
