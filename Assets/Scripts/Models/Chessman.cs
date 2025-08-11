using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Enums;
using Infrastructure;
using UnityEngine;

namespace Models
{
    public class Chessman : IChessPiece
    {
        public const float MovementDistance = 1.5f;

        public Board Board { get; set; }

        public string Name { get; set; }

        public int XBoard { get; set; } = -1;

        public int YBoard { get; set; } = -1;

        public PlayerSide Player { get; set; }

        public UnitRole Role { get; set; }

        public bool IsRemoved { get => Health <= 0; }

        public bool IsReady { get; set; } = true;


        // Tank stats

        private int _health = 3;
        public int Health
        {
            get => _health;
            set
            {
                if (value < 0)
                {
                    _health = 0;
                }
                else
                {
                    _health = value;
                }
            }
        }

        public int Movement { get; set; }

        public int Firepower { get; set; } = 4;

        public int Survivability { get; set; } = 2;

        public UnitDirection Direction { get; set; } = UnitDirection.Up;



        #region Constructors

        public Chessman() { }

        public Chessman(IChessPiece toCopy, Board board = null)
        {
            XBoard = toCopy.XBoard;
            YBoard = toCopy.YBoard;
            Player = toCopy.Player;
            Role = toCopy.Role;
            Health = toCopy.Health;
            Movement = toCopy.Movement;
            Firepower = toCopy.Firepower;
            Survivability = toCopy.Survivability;
            Board = board;
            Direction = toCopy.Direction;
            IsReady = toCopy.IsReady;
        }

        public Chessman(Chessman toCopy, Board board = null)
        {
            XBoard = toCopy.XBoard;
            YBoard = toCopy.YBoard;
            Player = toCopy.Player;
            Role = toCopy.Role;
            Health = toCopy.Health;
            Movement = toCopy.Movement;
            Firepower = toCopy.Firepower;
            Survivability = toCopy.Survivability;
            Board = board;
            Direction = toCopy.Direction;
            IsReady = toCopy.IsReady;
        }

        public Chessman(ChessmanComponent toCopy, Board board = null)
        {
            XBoard = toCopy.PieceInfo.XBoard;
            YBoard = toCopy.PieceInfo.YBoard;
            Player = toCopy.PieceInfo.Player;
            Role = toCopy.PieceInfo.Role;
            Health = toCopy.PieceInfo.Health;
            Movement = toCopy.PieceInfo.Movement;
            Firepower = toCopy.PieceInfo.Firepower;
            Survivability = toCopy.PieceInfo.Survivability;
            Board = board;
            Direction = toCopy.PieceInfo.Direction;
            IsReady = toCopy.PieceInfo.IsReady;
        }

        #endregion

        #region Piece movement patterns

        public virtual List<PossibleMove> GetPossibleMoves()
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
                case UnitRole.Rook:
                    moves = RookMovePattern();
                    break;
                case UnitRole.Pawn:
                    moves = SingleMovePattern(XBoard, YBoard, Direction);
                    break;
                default:
                    moves = new List<PossibleMove>();
                    break;
            }

            Board.GetPossibleTargets(moves);

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

                // Check fields on surrounding tiles to make sure you don't half-phase through terrain
                if (xIncrement != 0 && yIncrement != 0
                 && ((Board.IsPositionOnBoard(xStart, y) && Board.GetPosition(xStart, y) != null)
                 || (Board.IsPositionOnBoard(x, yStart) && Board.GetPosition(x, yStart) != null)))
                {
                    break;
                }

                result.Add(new PossibleMove(this, x, y, directions));

                x += xIncrement;
                y += yIncrement;
            }

            return result;
        }

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

            var vector = DirectionConverter.ConvertToVector2(directionStart);
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

            return result;
        }

        #endregion

        public Vector2 GetPositionVector2()
        {
            return new Vector2(XBoard, YBoard);
        }

        public (int X, int Y) GetPositionTupleXY()
        {
            return (XBoard, YBoard);
        }
    }
}