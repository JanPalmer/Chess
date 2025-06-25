using UnityEngine;

namespace Models
{
    public sealed class PossibleMove
    {
        public readonly Chessman ChessPiece;

        public readonly (int X, int Y) Start;
        public readonly (int X, int Y) End;

        // RemovedChessPiece is used as a flag for whether the move is an attack move or not
        // Don't use other constructors if the move is an attack move
        public readonly Chessman RemovedChessPiece = null;

        public PossibleMove(Chessman chessPiece, int xEnd, int yEnd)
        {
            ChessPiece = chessPiece;
            Start = (chessPiece.XBoard, chessPiece.YBoard);
            End = (xEnd, yEnd);
        }

        public PossibleMove(Chessman chessPiece, int xStart, int yStart, int xEnd, int yEnd)
        {
            ChessPiece = chessPiece;
            Start = (xStart, yStart);
            End = (xEnd, yEnd);
        }

        public PossibleMove(Chessman chessPiece, int xStart, int yStart, Chessman removedChessPiece)
        {
            ChessPiece = chessPiece;
            Start = (xStart, yStart);
            End = (removedChessPiece.XBoard, removedChessPiece.YBoard);
            RemovedChessPiece = removedChessPiece;
        }
    }
}