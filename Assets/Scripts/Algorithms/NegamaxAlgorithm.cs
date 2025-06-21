using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NegamaxAlgorithm : IAlgorithm
{
    private Dictionary<ChessPieceRole, int> _pieceValues = new Dictionary<ChessPieceRole, int>(){
        { ChessPieceRole.Pawn, 1},
        { ChessPieceRole.Bishop, 3},
        { ChessPieceRole.Knight, 3},
        { ChessPieceRole.Rook, 5},
        { ChessPieceRole.Queen, 8},
        { ChessPieceRole.King, 1000},
    };

    private PlayerSide _originalPlayer;
    private Chessman[,] _board;

    public (GameObject chessPieceToMove, int moveX, int moveY) CalculateNextMove(
        PlayerSide player,
        GameObject[,] positions,
        GameObject[] playerBlack,
        GameObject[] playerWhite)
    {
        _originalPlayer = player;
        var boardSizeX = positions.GetLength(0);
        var boardSizeY = positions.GetLength(1);
        _board = new Chessman[boardSizeX, boardSizeY];
        for (int y = 0; y < boardSizeY; y++)
        {
            for (int x = 0; x < boardSizeX; x++)
            {
                var chessPiece = positions[x, y];
                if (chessPiece != null)
                {
                    _board[x, y] = new Chessman(chessPiece.GetComponent<Chessman>());
                }
            }
        }
    }

    private int NegaMax(int depth)
    {


        if (depth == 0)
        {
            int currentPlayer = (int)_originalPlayer * (depth + 1) % 2;
            return Evaluate((PlayerSide)currentPlayer);
        }

        var max = int.MinValue;

        foreach
    }

    private int Evaluate(PlayerSide currentlyEvaluatedPlayer)
    {
        return Convert.ToInt32(_originalPlayer == currentlyEvaluatedPlayer);
    }

    private void MakeMove()
    {

    }

    private void UndoMove()
    {

    }
}
