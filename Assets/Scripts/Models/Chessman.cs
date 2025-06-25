using System.Collections.Generic;
using Components;
using UnityEngine;

namespace Models
{
    public class Chessman : ScriptableObject
    {
        /// <summary>
        /// Parent Board to which the piece belongs to
        /// </summary>
        public Board Board { get; set; }

        /// <summary>
        /// Position on the board - X coordinate
        /// </summary>
        public int XBoard { get; set; } = -1;
        /// <summary>
        /// Position on the board - Y coordinate
        /// </summary>
        public int YBoard { get; set; } = -1;

        /// <summary>
        /// Variable to keep track if player is "black" or "white"
        /// </summary>
        public PlayerSide Player { get; set; }

        /// <summary>
        /// Piece role, like 'Pawn', 'Rook' etc
        /// </summary>
        public ChessPieceRole Role { get; set; }

        /// <summary>
        /// Marks the piece as removed
        /// </summary>
        public bool IsRemoved { get; set; } = false;

        #region Constructors

        public Chessman() { }

        public Chessman(Chessman toCopy, Board board = null)
        {
            XBoard = toCopy.XBoard;
            YBoard = toCopy.YBoard;
            Player = toCopy.Player;
            Role = toCopy.Role;
            IsRemoved = toCopy.IsRemoved;
            Board = board;
        }

        public Chessman(ChessmanObject toCopy, Board board = null)
        {
            XBoard = toCopy.ChessPieceInformation.XBoard;
            YBoard = toCopy.ChessPieceInformation.YBoard;
            Player = toCopy.ChessPieceInformation.Player;
            Role = toCopy.ChessPieceInformation.Role;
            IsRemoved = toCopy.ChessPieceInformation.IsRemoved;
            Board = board;
        }

        #endregion

        #region Piece movement patterns

        public List<PossibleMove> GetPossibleMoves()
        {
            var moves = new List<PossibleMove>();

            switch (Role)
            {
                case ChessPieceRole.Queen:
                    moves.AddRange(LineMovePattern(1, 0));
                    moves.AddRange(LineMovePattern(1, 1));
                    moves.AddRange(LineMovePattern(0, 1));
                    moves.AddRange(LineMovePattern(-1, 1));
                    moves.AddRange(LineMovePattern(-1, 0));
                    moves.AddRange(LineMovePattern(-1, -1));
                    moves.AddRange(LineMovePattern(0, -1));
                    moves.AddRange(LineMovePattern(1, -1));
                    break;
                case ChessPieceRole.Knight:
                    moves = LMovePattern();
                    break;
                case ChessPieceRole.Bishop:
                    moves.AddRange(LineMovePattern(1, 1));
                    moves.AddRange(LineMovePattern(1, -1));
                    moves.AddRange(LineMovePattern(-1, 1));
                    moves.AddRange(LineMovePattern(-1, -1));
                    break;
                case ChessPieceRole.King:
                    moves = SurroundMovePattern();
                    break;
                case ChessPieceRole.Rook:
                    moves.AddRange(LineMovePattern(1, 0));
                    moves.AddRange(LineMovePattern(0, 1));
                    moves.AddRange(LineMovePattern(-1, 0));
                    moves.AddRange(LineMovePattern(0, -1));
                    break;
                case ChessPieceRole.Pawn:
                    if (Player == PlayerSide.Black)
                    {
                        moves = PawnMovePattern(XBoard, YBoard - 1);
                    }
                    else
                    {
                        moves = PawnMovePattern(XBoard, YBoard + 1);
                    }
                    break;
            }

            return moves;
        }

        public List<PossibleMove> LineMovePattern(int xIncrement, int yIncrement)
        {
            int x = XBoard + xIncrement;
            int y = YBoard + yIncrement;

            var result = new List<PossibleMove>();

            while (Board.IsPositionOnBoard(x, y) && Board.GetPosition(x, y) == null)
            {
                result.Add(new PossibleMove(this, x, y));

                x += xIncrement;
                y += yIncrement;
            }

            if (Board.IsPositionOnBoard(x, y))
            {
                var pieceOnBoard = Board.GetPosition(x, y);
                if (pieceOnBoard != null && pieceOnBoard.Player != Player)
                {
                    result.Add(new PossibleMove(this, x, y, pieceOnBoard));
                }
            }

            return result;
        }

        public List<PossibleMove> LMovePattern()
        {
            var result = new List<PossibleMove>();
            var possibleVectors = new List<(int x, int y)>(){
            (1, 2),
            (-1, 2),
            (2, 1),
            (-2, 1),
            (1, -2),
            (-1, -2),
            (2, -1),
            (-2, -1),
        };

            foreach (var vector in possibleVectors)
            {
                if (PointMovePlate(XBoard + vector.x, YBoard + vector.y, out var possibleMove))
                {
                    result.Add(possibleMove);
                }
            }

            return result;
        }

        public List<PossibleMove> SurroundMovePattern()
        {
            var possibleMoves = new List<PossibleMove>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    if (PointMovePlate(XBoard + i, YBoard + j, out var possibleMove))
                    {
                        possibleMoves.Add(possibleMove);
                    }
                }
            }

            return possibleMoves;
        }

        public bool PointMovePlate(int x, int y, out PossibleMove possibleMove)
        {
            if (Board.IsPositionOnBoard(x, y))
            {
                var pieceToRemove = Board.GetPosition(x, y);

                if (pieceToRemove == null)
                {
                    possibleMove = new PossibleMove(this, x, y);
                    return true;
                }
                else if (pieceToRemove.Player != Player)
                {
                    possibleMove = new PossibleMove(this, x, y, pieceToRemove);
                    return true;
                }
            }

            possibleMove = null;
            return false;
        }

        public List<PossibleMove> PawnMovePattern(int x, int y)
        {
            var result = new List<PossibleMove>();

            if (Board.IsPositionOnBoard(x, y))
            {
                var pieceToRemove = Board.GetPosition(x, y);

                // Move forward
                if (pieceToRemove == null)
                {
                    result.Add(new PossibleMove(this, x, y));
                }

                // Or attack diagonally
                if (Board.IsPositionOnBoard(x + 1, y)
                && Board.GetPosition(x + 1, y) != null
                && Board.GetPosition(x + 1, y).Player != Player)
                {
                    result.Add(new PossibleMove(this, x + 1, y, Board.GetPosition(x + 1, y)));
                }

                if (Board.IsPositionOnBoard(x - 1, y)
                && Board.GetPosition(x - 1, y) != null
                && Board.GetPosition(x - 1, y).Player != Player)
                {
                    result.Add(new PossibleMove(this, x - 1, y, Board.GetPosition(x - 1, y)));
                }
            }

            return result;
        }

        #endregion
    }
}