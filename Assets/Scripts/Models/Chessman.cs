using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Enums;
using Unity.VisualScripting;
using UnityEngine;

namespace Models
{
    public class Chessman
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


        // Tank stats

        public uint Health { get; set; }

        public uint Movement { get; set; }

        public uint Firepower { get; set; }

        public uint Survivability { get; set; }

        public ChessPieceDirection Direction { get; set; } = ChessPieceDirection.Up;



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
            Direction = toCopy.Direction;
        }

        public Chessman(ChessmanComponent toCopy, Board board = null)
        {
            XBoard = toCopy.ChessPieceInfo.XBoard;
            YBoard = toCopy.ChessPieceInfo.YBoard;
            Player = toCopy.ChessPieceInfo.Player;
            Role = toCopy.ChessPieceInfo.Role;
            IsRemoved = toCopy.ChessPieceInfo.IsRemoved;
            Board = board;
            Direction = toCopy.ChessPieceInfo.Direction;
        }

        #endregion

        #region Piece movement patterns

        public List<PossibleMove> GetPossibleMoves()
        {
            var moves = new List<PossibleMove>();

            switch (Role)
            {
                // case ChessPieceRole.Queen:
                //     moves.AddRange(LineMovePattern(1, 0));
                //     moves.AddRange(LineMovePattern(1, 1));
                //     moves.AddRange(LineMovePattern(0, 1));
                //     moves.AddRange(LineMovePattern(-1, 1));
                //     moves.AddRange(LineMovePattern(-1, 0));
                //     moves.AddRange(LineMovePattern(-1, -1));
                //     moves.AddRange(LineMovePattern(0, -1));
                //     moves.AddRange(LineMovePattern(1, -1));
                //     break;
                // case ChessPieceRole.Knight:
                //     moves = LMovePattern();
                //     break;
                // case ChessPieceRole.Bishop:
                //     moves.AddRange(LineMovePattern(1, 1));
                //     moves.AddRange(LineMovePattern(1, -1));
                //     moves.AddRange(LineMovePattern(-1, 1));
                //     moves.AddRange(LineMovePattern(-1, -1));
                //     break;
                // case ChessPieceRole.King:
                //     moves = SurroundMovePattern();
                //     break;
                // case ChessPieceRole.Rook:
                //     moves.AddRange(LineMovePattern(1, 0));
                //     moves.AddRange(LineMovePattern(0, 1));
                //     moves.AddRange(LineMovePattern(-1, 0));
                //     moves.AddRange(LineMovePattern(0, -1));
                //     break;
                // case ChessPieceRole.Pawn:
                //     if (Player == PlayerSide.Black)
                //     {
                //         moves = PawnMovePattern(XBoard, YBoard - 1);
                //     }
                //     else
                //     {
                //         moves = PawnMovePattern(XBoard, YBoard + 1);
                //     }
                //     break;
                case ChessPieceRole.Rook:
                    moves = RookMovePattern();
                    break;
                case ChessPieceRole.Pawn:
                    moves = SingleMovePattern(XBoard, YBoard, Direction);
                    break;
                default:
                    moves = new List<PossibleMove>();
                    break;
            }

            return moves;
        }

        public List<PossibleMove> LineMovePattern(
            int xIncrement,
            int yIncrement,
            IEnumerable<ChessPieceDirection> directions,
            int movementRadius)
        {
            int x = XBoard + xIncrement;
            int y = YBoard + yIncrement;

            var result = new List<PossibleMove>();

            float DistanceFromSource()
            {
                var xDist = x - XBoard;
                var yDist = y - YBoard;
                return (float)Math.Sqrt(xDist * xDist + yDist * yDist);
            }

            while (DistanceFromSource() <= movementRadius
                && Board.IsPositionOnBoard(x, y)
                && Board.GetPosition(x, y) == null)
            {
                result.Add(new PossibleMove(this, x, y, directions));

                x += xIncrement;
                y += yIncrement;
            }

            // if (Board.IsPositionOnBoard(x, y))
            // {
            //     var pieceOnBoard = Board.GetPosition(x, y);
            //     if (pieceOnBoard != null && pieceOnBoard.Player != Player)
            //     {
            //         result.Add(new PossibleMove(this, pieceOnBoard, directions));
            //     }
            // }

            return result;
        }

        // public List<PossibleMove> LMovePattern()
        // {
        //     var result = new List<PossibleMove>();
        //     var possibleVectors = new List<(int x, int y)>(){
        //     (1, 2),
        //     (-1, 2),
        //     (2, 1),
        //     (-2, 1),
        //     (1, -2),
        //     (-1, -2),
        //     (2, -1),
        //     (-2, -1),
        // };

        //     foreach (var vector in possibleVectors)
        //     {
        //         if (PointMovePlate(XBoard + vector.x, YBoard + vector.y, out var possibleMove))
        //         {
        //             result.Add(possibleMove);
        //         }
        //     }

        //     return result;
        // }

        // public List<PossibleMove> SurroundMovePattern()
        // {
        //     var possibleMoves = new List<PossibleMove>();

        //     for (int i = -1; i <= 1; i++)
        //     {
        //         for (int j = -1; j <= 1; j++)
        //         {
        //             if (i == 0 && j == 0)
        //             {
        //                 continue;
        //             }

        //             if (PointMovePlate(XBoard + i, YBoard + j, out var possibleMove))
        //             {
        //                 possibleMoves.Add(possibleMove);
        //             }
        //         }
        //     }

        //     return possibleMoves;
        // }

        // public bool PointMovePlate(int x, int y, out PossibleMove possibleMove)
        // {
        //     if (Board.IsPositionOnBoard(x, y))
        //     {
        //         var pieceToRemove = Board.GetPosition(x, y);

        //         if (pieceToRemove == null)
        //         {
        //             possibleMove = new PossibleMove(this, x, y);
        //             return true;
        //         }
        //         else if (pieceToRemove.Player != Player)
        //         {
        //             possibleMove = new PossibleMove(this, pieceToRemove);
        //             return true;
        //         }
        //     }

        //     possibleMove = null;
        //     return false;
        // }

        public List<PossibleMove> RookMovePattern()
        {
            var baseMoves = SingleMovePattern(XBoard, YBoard, Direction);
            var resultArrows = new List<DirectionArrow>();

            var directions = baseMoves.SelectMany(x => x.Directions).ToList();

            resultArrows.AddRange(directions);

            foreach (var direction in directions)
            {
                var nextDepthMoves = SingleMovePattern(direction.Move.End.X, direction.Move.End.Y, direction.Direction);
                var nextDepthDirections = nextDepthMoves.SelectMany(x => x.Directions).ToList();
                foreach (var arrow in nextDepthDirections)
                {
                    if (directions.Any(x => x.Move.End.X == arrow.Move.End.X && x.Move.End.Y == arrow.Move.End.Y && x.Direction == arrow.Direction))
                    {
                        continue;
                    }
                    else
                    {
                        arrow.Depth = 2;
                        arrow.Move.PrecedingMoves.Add(direction.Move);
                        resultArrows.Add(arrow);
                    }
                }
            }

            var resultMoves = resultArrows.Select(x => x.Move).ToList();



            return resultMoves;
        }

        public List<PossibleMove> SingleMovePattern(int xStart, int yStart, ChessPieceDirection directionStart)
        {
            var result = new List<PossibleMove>();

            var vector = DirectionConverter.Convert(directionStart);
            List<PossibleMove> moves;
            List<Vector2> directions;

            switch (vector)
            {
                case (0, 1):
                    Debug.Log("Up");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + 0, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Up, ChessPieceDirection.UpperLeft, ChessPieceDirection.UpperRight }),
                        new PossibleMove(this, xStart + 0, yStart + 2, new List<ChessPieceDirection>(){ ChessPieceDirection.Up, ChessPieceDirection.UpperLeft, ChessPieceDirection.UpperRight }),
                        new PossibleMove(this, xStart + 1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.UpperRight, ChessPieceDirection.Right }),
                        new PossibleMove(this, xStart + -1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.UpperLeft, ChessPieceDirection.Left }),
                        new PossibleMove(this, xStart + 0, yStart - 1, new List<ChessPieceDirection>(){ChessPieceDirection.Up, ChessPieceDirection.UpperLeft, ChessPieceDirection.UpperRight }),
                    };
                    break;
                case (0, -1):
                    Debug.Log("Down");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + 0, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Down, ChessPieceDirection.LowerLeft, ChessPieceDirection.LowerRight }),
                        new PossibleMove(this, xStart + 0, yStart + -2, new List<ChessPieceDirection>(){ ChessPieceDirection.Down, ChessPieceDirection.LowerLeft, ChessPieceDirection.LowerRight }),
                        new PossibleMove(this, xStart + -1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.LowerLeft, ChessPieceDirection.Left }),
                        new PossibleMove(this, xStart + 1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.LowerRight, ChessPieceDirection.Right }),
                        new PossibleMove(this, xStart + 0, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Down, ChessPieceDirection.LowerLeft, ChessPieceDirection.LowerRight }),
                    };
                    break;
                case (1, 0):
                    Debug.Log("Right");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + 1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.UpperRight, ChessPieceDirection.LowerRight }),
                        new PossibleMove(this, xStart + 2, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.UpperRight, ChessPieceDirection.LowerRight }),
                        new PossibleMove(this, xStart + 1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.UpperRight, ChessPieceDirection.Up }),
                        new PossibleMove(this, xStart + 1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.LowerRight, ChessPieceDirection.Down }),
                        new PossibleMove(this, xStart + -1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.UpperRight, ChessPieceDirection.LowerRight }),
                    };
                    break;
                case (-1, 0):
                    Debug.Log("Left");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + -1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.UpperLeft, ChessPieceDirection.LowerLeft }),
                        new PossibleMove(this, xStart + -2, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.UpperLeft, ChessPieceDirection.LowerLeft }),
                        new PossibleMove(this, xStart + -1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.LowerLeft, ChessPieceDirection.Down }),
                        new PossibleMove(this, xStart + -1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.UpperLeft, ChessPieceDirection.Up }),
                        new PossibleMove(this, xStart + 1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.UpperLeft, ChessPieceDirection.LowerLeft }),
                    };
                    break;
                case (1, 1):
                    Debug.Log("UpperRight");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + 1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Up, ChessPieceDirection.UpperRight, ChessPieceDirection.Right }),
                        new PossibleMove(this, xStart + 0, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.UpperLeft, ChessPieceDirection.Up }),
                        new PossibleMove(this, xStart + 1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.LowerRight }),
                        new PossibleMove(this, xStart + -1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Up, ChessPieceDirection.UpperRight, ChessPieceDirection.Right }),
                    };
                    break;
                case (-1, 1):
                    Debug.Log("UpperLeft");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + -1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.UpperLeft, ChessPieceDirection.Up }),
                        new PossibleMove(this, xStart + 0, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Up, ChessPieceDirection.UpperRight }),
                        new PossibleMove(this, xStart + -1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.LowerLeft }),
                        new PossibleMove(this, xStart + 1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.UpperLeft, ChessPieceDirection.Up }),
                    };
                    break;
                case (1, -1):
                    Debug.Log("LowerRight");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + 1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.LowerRight, ChessPieceDirection.Down }),
                        new PossibleMove(this, xStart + 0, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Down, ChessPieceDirection.LowerLeft }),
                        new PossibleMove(this, xStart + 1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.UpperRight }),
                        new PossibleMove(this, xStart + -1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Right, ChessPieceDirection.LowerRight, ChessPieceDirection.Down }),
                    };
                    break;
                case (-1, -1):
                    Debug.Log("LowerLeft");
                    moves = new List<PossibleMove>
                    {
                        new PossibleMove(this, xStart + -1, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.LowerLeft, ChessPieceDirection.Down }),
                        new PossibleMove(this, xStart + 0, yStart + -1, new List<ChessPieceDirection>(){ ChessPieceDirection.Down, ChessPieceDirection.LowerRight }),
                        new PossibleMove(this, xStart + -1, yStart + 0, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.UpperLeft }),
                        new PossibleMove(this, xStart + 1, yStart + 1, new List<ChessPieceDirection>(){ ChessPieceDirection.Left, ChessPieceDirection.LowerLeft, ChessPieceDirection.Down }),
                    };
                    break;
                default:
                    moves = new List<PossibleMove>();
                    break;
            }

            foreach (var move in moves)
            {
                Debug.Log($"PawnMovePattern - move {move.End.X}, {move.End.Y}");

                if (Board.IsPositionOnBoard(move.End.X, move.End.Y) && Board.GetPosition(move.End.X, move.End.Y) == null)
                {
                    result.Add(move);
                }
            }

            // if ((vector.X & 1) == 0 || (vector.Y & 1) == 0)
            // {
            //     var fields = new List<(int x, int y)> { (1, 0), (2, 0), (1, -1), (1, 1) };

            //     var directionVector = new Vector2(vector.X - 1, vector.Y - 0);
            //     directionVector.Normalize();

            //     float angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;

            //     if (angle != 0)
            //     {
            //         for (int i = 0; i < fields.Count; i++)
            //         {
            //             var field = fields.First();
            //             var fieldVector = Quaternion.Euler(0, 0, angle) * new Vector2(field.x, field.y);
            //             fields.RemoveAt(0);
            //             fields.Add(((int)fieldVector.x, (int)fieldVector.y));
            //         }
            //     }
            // }

            return result;
        }

        #endregion
    }
}