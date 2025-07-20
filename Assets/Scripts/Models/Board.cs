using System.Collections.Generic;
using Components;
using Enums;
using Models;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Board", menuName = "Scriptable Objects/Board")]
public class Board
{
    public IChessPiece[,] Positions { get; set; } = new IChessPiece[8, 8];

    // public List<Chessman> PlayerBlack { get; } = new List<Chessman>(16);
    // public List<Chessman> PlayerWhite { get; } = new List<Chessman>(16);

    public void SetPosition(IChessPiece chesspiece, int x, int y)
    {
        chesspiece.XBoard = x;
        chesspiece.YBoard = y;

        Positions[chesspiece.XBoard, chesspiece.YBoard] = chesspiece;
    }

    public void SetPosition(GameObject obj, int x, int y)
    {
        var chesspiece = obj.GetComponent<ChessmanComponent>().PieceInfo;
        SetPosition(chesspiece, x, y);
    }

    public void SetPositionEmpty(int x, int y)
    {
        Positions[x, y] = null;
    }

    public IChessPiece GetPosition(int x, int y)
    {
        return Positions[x, y];
    }

    public bool IsPositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Positions.GetLength(0) || y >= Positions.GetLength(1))
        {
            return false;
        }

        return true;
    }

    public Chessman MoveChessPiece(PossibleMove move, UnitDirection direction)
    {
        //Debug.Log($"Move - {move.Start.X}, {move.Start.Y} -> {move.End.X}, {move.End.Y}");

        SetPosition(move.ChessPiece, move.End.X, move.End.Y);
        SetPositionEmpty(move.Start.X, move.Start.Y);

        if (move.RemovedChessPiece != null)
        {
            move.RemovedChessPiece.IsRemoved = true;
            //Debug.Log($"Removed {move.RemovedChessPiece.Role}");
        }

        move.ChessPiece.Direction = direction;

        return move.RemovedChessPiece;
    }

    public Chessman UndoMove(PossibleMove move)
    {
        //Debug.Log($"Undoing move - {move.Start.X}, {move.Start.Y} -> {move.End.X}, {move.End.Y}");

        SetPosition(move.ChessPiece, move.Start.X, move.Start.Y);
        SetPositionEmpty(move.End.X, move.End.Y);

        if (move.RemovedChessPiece != null)
        {
            move.RemovedChessPiece.IsRemoved = false;
            SetPosition(move.RemovedChessPiece, move.End.X, move.End.Y);
        }

        return move.RemovedChessPiece;
    }
}
