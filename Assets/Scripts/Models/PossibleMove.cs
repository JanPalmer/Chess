using System.Collections.Generic;
using System.Linq;
using Enums;
using Unity.VisualScripting;
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

        public readonly List<DirectionArrow> Directions;

        public readonly List<PossibleMove> PrecedingMoves = new List<PossibleMove>();


        public PossibleMove(PossibleMove moveToCopy)
        {
            ChessPiece = moveToCopy.ChessPiece;
            Start = (moveToCopy.Start.X, moveToCopy.Start.Y);
            End = (moveToCopy.End.X, moveToCopy.End.Y);
            RemovedChessPiece = moveToCopy.RemovedChessPiece;
            Directions = CopyDirectionArrows(moveToCopy.Directions);
        }

        public PossibleMove(Chessman chessPiece, int xEnd, int yEnd, IEnumerable<ChessPieceDirection> directions)
        {
            ChessPiece = chessPiece;
            Start = (chessPiece.XBoard, chessPiece.YBoard);
            End = (xEnd, yEnd);
            Directions = CreateDirectionArrows(directions);
        }

        public PossibleMove(Chessman chessPiece, int xStart, int yStart, int xEnd, int yEnd, IEnumerable<ChessPieceDirection> directions)
        {
            ChessPiece = chessPiece;
            Start = (xStart, yStart);
            End = (xEnd, yEnd);
            Directions = CreateDirectionArrows(directions);
        }

        private List<DirectionArrow> CreateDirectionArrows(IEnumerable<ChessPieceDirection> directions)
        {
            var result = new List<DirectionArrow>();

            foreach (var direction in directions)
            {
                var copiedDirection = new DirectionArrow()
                {
                    Move = this,
                    Direction = direction,
                };
                result.Add(copiedDirection);
            }

            return result;
        }

        private List<DirectionArrow> CopyDirectionArrows(List<DirectionArrow> toCopy)
        {
            var result = new List<DirectionArrow>();

            foreach (var arrow in toCopy)
            {
                var copiedDirection = new DirectionArrow()
                {
                    Move = this,
                    Direction = arrow.Direction,
                    Depth = arrow.Depth,
                };
                Directions.Add(copiedDirection);
            }

            return result;
        }

        // public PossibleMove(Chessman chessPiece, Chessman removedChessPiece)
        // {
        //     ChessPiece = chessPiece;
        //     Start = (chessPiece.XBoard, chessPiece.YBoard);
        //     End = (removedChessPiece.XBoard, removedChessPiece.YBoard);
        //     RemovedChessPiece = removedChessPiece;
        // }
    }
}