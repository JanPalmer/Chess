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
        // The 0.5 is for turning/movement on the skewed axes, since for 1.0 the distance to the left and right is not registered,
        // while putting 2.0 gives too much movement forward
        public const float MovementDistance = 1.5f;

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

        public UnitDirection Direction { get; set; } = UnitDirection.Up;



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
            int xStart,
            int yStart,
            int xIncrement,
            int yIncrement,
            IEnumerable<UnitDirection> directions,
            float movementRadius)
        {
            int x = xStart + xIncrement;
            int y = yStart + yIncrement;

            var result = new List<PossibleMove>();

            // Helper function to determine distance from starting position
            float DistanceFromSource()
            {
                var xDist = x - xStart;
                var yDist = y - yStart;
                return (float)Math.Sqrt(xDist * xDist + yDist * yDist);
            }

            while (Board.IsPositionOnBoard(x, y)
                && Board.GetPosition(x, y) == null
                && DistanceFromSource() <= movementRadius)
            {
                result.Add(new PossibleMove(this, x, y, directions));

                x += xIncrement;
                y += yIncrement;
            }

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
                //Debug.Log($"Direction - {direction.Move.End.X}, {direction.Move.End.Y}, {direction.Direction}");

                var nextDepthMoves = SingleMovePattern(direction.Move.End.X, direction.Move.End.Y, direction.Direction);
                var nextDepthDirections = nextDepthMoves.SelectMany(x => x.Directions).ToList();
                foreach (var arrow in nextDepthDirections)
                {
                    //Debug.Log($"Next Depth Direction - {arrow.Move.End.X}, {arrow.Move.End.Y}, {arrow.Direction}");
                    var possibleSameMove = directions.FirstOrDefault(x => x.Move.End.X == arrow.Move.End.X && x.Move.End.Y == arrow.Move.End.Y);
                    if (possibleSameMove != null)
                    {
                        if (possibleSameMove.Move.Directions.Select(x => x.Direction == arrow.Direction) != null)
                        {
                            continue;
                        }
                        arrow.Move = possibleSameMove.Move;
                        possibleSameMove.Move.Directions.Add(arrow);
                    }
                    else
                    {
                    }

                    resultArrows.Add(arrow);
                    arrow.Move.PrecedingMoves.Add(direction);
                    arrow.Depth = 2;
                }
            }

            var resultMoves = resultArrows.Select(x => x.Move).ToList();



            return resultMoves;
        }

        public List<PossibleMove> SingleMovePattern(int xStart, int yStart, UnitDirection directionStart)
        {
            var result = new List<PossibleMove>();

            var vector = DirectionConverter.Convert(directionStart);
            List<(Vector2 vector, List<UnitDirection> directions)> directionsForward;
            (Vector2 vector, List<UnitDirection> directions) directionBackward;

            switch (vector)
            {
                case (0, 1):
                    //Debug.Log("Up");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(1, 1), new List<UnitDirection>(){ UnitDirection.UpperRight, UnitDirection.Right } ),
                        ( new Vector2(0, 1), new List<UnitDirection>(){ UnitDirection.Up, UnitDirection.UpperLeft, UnitDirection.UpperRight } ),
                        ( new Vector2(-1, 1), new List<UnitDirection>(){ UnitDirection.UpperLeft, UnitDirection.Left } ),
                    };
                    directionBackward = (new Vector2(0, -1), new List<UnitDirection>() { UnitDirection.Up, UnitDirection.UpperLeft, UnitDirection.UpperRight });
                    break;
                case (0, -1):
                    //Debug.Log("Down");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(1, -1), new List<UnitDirection>(){ UnitDirection.LowerRight, UnitDirection.Right } ),
                        ( new Vector2(0, -1), new List<UnitDirection>(){ UnitDirection.Down, UnitDirection.LowerLeft, UnitDirection.LowerRight } ),
                        ( new Vector2(-1, -1), new List<UnitDirection>(){ UnitDirection.LowerLeft, UnitDirection.Left } ),
                    };
                    directionBackward = (new Vector2(0, 1), new List<UnitDirection>() { UnitDirection.Down, UnitDirection.LowerLeft, UnitDirection.LowerRight });
                    break;
                case (1, 0):
                    //Debug.Log("Right");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(1, 1), new List<UnitDirection>(){ UnitDirection.UpperRight, UnitDirection.Up } ),
                        ( new Vector2(1, 0), new List<UnitDirection>(){ UnitDirection.Right, UnitDirection.UpperRight, UnitDirection.LowerRight } ),
                        ( new Vector2(1, -1), new List<UnitDirection>(){ UnitDirection.LowerRight, UnitDirection.Down } ),
                    };
                    directionBackward = (new Vector2(-1, 0), new List<UnitDirection>() { UnitDirection.Right, UnitDirection.UpperRight, UnitDirection.LowerRight });
                    break;
                case (-1, 0):
                    //Debug.Log("Left");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(-1, 1), new List<UnitDirection>(){ UnitDirection.UpperLeft, UnitDirection.Up } ),
                        ( new Vector2(-1, 0), new List<UnitDirection>(){ UnitDirection.Left, UnitDirection.UpperLeft, UnitDirection.LowerLeft } ),
                        ( new Vector2(-1, -1), new List<UnitDirection>(){ UnitDirection.LowerLeft, UnitDirection.Down } ),
                    };
                    directionBackward = (new Vector2(1, 0), new List<UnitDirection>() { UnitDirection.Left, UnitDirection.UpperLeft, UnitDirection.LowerLeft });
                    break;
                case (1, 1):
                    //Debug.Log("UpperRight");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(0, 1), new List<UnitDirection>(){ UnitDirection.UpperLeft, UnitDirection.Up } ),
                        ( new Vector2(1, 1), new List<UnitDirection>(){ UnitDirection.Up, UnitDirection.UpperRight, UnitDirection.Right } ),
                        ( new Vector2(1, 0), new List<UnitDirection>(){ UnitDirection.Right, UnitDirection.LowerRight } ),
                    };
                    directionBackward = (new Vector2(-1, -1), new List<UnitDirection>() { UnitDirection.Up, UnitDirection.UpperRight, UnitDirection.Right });
                    break;
                case (-1, 1):
                    //Debug.Log("UpperLeft");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(0, 1), new List<UnitDirection>(){ UnitDirection.Up, UnitDirection.UpperRight } ),
                        ( new Vector2(-1, 1), new List<UnitDirection>(){ UnitDirection.Left, UnitDirection.UpperLeft, UnitDirection.Up } ),
                        ( new Vector2(-1, 0), new List<UnitDirection>(){ UnitDirection.Left, UnitDirection.LowerLeft } ),
                    };
                    directionBackward = (new Vector2(1, -1), new List<UnitDirection>() { UnitDirection.Left, UnitDirection.UpperLeft, UnitDirection.Up });
                    break;
                case (1, -1):
                    //Debug.Log("LowerRight");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(0, -1), new List<UnitDirection>(){ UnitDirection.Down, UnitDirection.LowerLeft } ),
                        ( new Vector2(1, -1), new List<UnitDirection>(){ UnitDirection.Right, UnitDirection.LowerRight, UnitDirection.Down } ),
                        ( new Vector2(1, 0), new List<UnitDirection>(){ UnitDirection.Right, UnitDirection.UpperRight } ),
                    };
                    directionBackward = (new Vector2(-1, 1), new List<UnitDirection>() { UnitDirection.Right, UnitDirection.LowerRight, UnitDirection.Down });
                    break;
                case (-1, -1):
                    //Debug.Log("LowerLeft");
                    directionsForward = new List<(Vector2, List<UnitDirection>)>()
                    {
                        ( new Vector2(0, -1), new List<UnitDirection>(){ UnitDirection.Down, UnitDirection.LowerRight } ),
                        ( new Vector2(-1, -1), new List<UnitDirection>(){ UnitDirection.Left, UnitDirection.LowerLeft, UnitDirection.Down } ),
                        ( new Vector2(-1, 0), new List<UnitDirection>(){ UnitDirection.Left, UnitDirection.UpperLeft } ),
                    };
                    directionBackward = (new Vector2(1, 1), new List<UnitDirection>() { UnitDirection.Left, UnitDirection.LowerLeft, UnitDirection.Down });
                    break;
                default:
                    directionsForward = new List<(Vector2, List<UnitDirection>)>();
                    directionBackward = (new Vector2(0, 0), new List<UnitDirection>());
                    break;
            }

            foreach (var direction in directionsForward)
            {
                result.AddRange(LineMovePattern(xStart, yStart, (int)direction.vector.x, (int)direction.vector.y, direction.directions, MovementDistance));
            }

            result.AddRange(LineMovePattern(xStart, yStart, (int)directionBackward.vector.x, (int)directionBackward.vector.y, directionBackward.directions, MovementDistance));
            result.Add(new PossibleMove(this, XBoard, YBoard, directionBackward.directions));
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