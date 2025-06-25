using System.Collections.Generic;
using Components;
using Models;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Board", menuName = "Scriptable Objects/Board")]
public class Board : ScriptableObject
{
    public Chessman[,] Positions { get; set; } = new Chessman[8, 8];

    // public List<Chessman> PlayerBlack { get; } = new List<Chessman>(16);
    // public List<Chessman> PlayerWhite { get; } = new List<Chessman>(16);

    public void SetPosition(Chessman chesspiece, int x, int y)
    {
        chesspiece.XBoard = x;
        chesspiece.YBoard = y;

        Positions[chesspiece.XBoard, chesspiece.YBoard] = chesspiece;
    }

    public void SetPosition(GameObject obj, int x, int y)
    {
        var chesspiece = obj.GetComponent<ChessmanObject>().ChessPieceInformation;
        SetPosition(chesspiece, x, y);
    }

    public void SetPositionEmpty(int x, int y)
    {
        Positions[x, y] = null;
    }

    public Chessman GetPosition(int x, int y)
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

    public Chessman MoveChessPiece(PossibleMove move)
    {
        SetPosition(move.ChessPiece, move.End.X, move.End.Y);
        SetPositionEmpty(move.Start.X, move.Start.Y);

        if (move.RemovedChessPiece != null)
        {
            move.RemovedChessPiece.IsRemoved = true;
        }

        return move.RemovedChessPiece;
    }
}
