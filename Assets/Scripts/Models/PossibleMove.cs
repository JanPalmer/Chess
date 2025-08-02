using System.Collections.Generic;
using System.Linq;
using Enums;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Models
{
    public sealed class PossibleMove
    {
        public readonly Chessman ChessPiece;

        public readonly (int X, int Y) Start;
        public readonly UnitDirection StartingDirection;

        public readonly (int X, int Y) End;

        // RemovedChessPiece is used as a flag for whether the move is an attack move or not
        // Don't use other constructors if the move is an attack move
        public readonly Chessman RemovedChessPiece = null;
        public readonly UnitVisibility TargetVisibility;

        // not readonly, as it's assigned later
        public List<(IChessPiece PossibleTarget, UnitVisibility Visibility)> Targets = null;

        public readonly List<DirectionArrow> Directions;

        public readonly List<DirectionArrow> PrecedingMoves = new List<DirectionArrow>();

        public override bool Equals(object obj)
        {
            if (obj is not PossibleMove)
            {
                return false;
            }

            var moveToCheck = obj as PossibleMove;

            return this.ChessPiece == moveToCheck.ChessPiece
                && this.Start.X == moveToCheck.Start.X
                && this.Start.Y == moveToCheck.Start.Y
                && this.End.X == moveToCheck.End.X
                && this.End.Y == moveToCheck.End.Y;
        }

        public PossibleMove(PossibleMove moveToCopy)
        {
            ChessPiece = moveToCopy.ChessPiece;
            StartingDirection = moveToCopy.ChessPiece.Direction;
            Start = (moveToCopy.Start.X, moveToCopy.Start.Y);
            End = (moveToCopy.End.X, moveToCopy.End.Y);
            Targets = new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>(moveToCopy.Targets);
            Directions = CopyDirectionArrows(moveToCopy.Directions);
        }

        public PossibleMove(Chessman chessPiece, int xEnd, int yEnd, IEnumerable<UnitDirection> directions)
        {
            ChessPiece = chessPiece;
            StartingDirection = chessPiece.Direction;
            Start = (chessPiece.XBoard, chessPiece.YBoard);
            End = (xEnd, yEnd);
            Directions = CreateDirectionArrows(directions);
        }

        public PossibleMove(Chessman chessPiece, int xStart, int yStart, int xEnd, int yEnd, IEnumerable<UnitDirection> directions)
        {
            ChessPiece = chessPiece;
            StartingDirection = chessPiece.Direction;
            Start = (xStart, yStart);
            End = (xEnd, yEnd);
            Directions = CreateDirectionArrows(directions);
        }

        public PossibleMove(
            Chessman chessPiece,
            int xEnd,
            int yEnd,
            IEnumerable<UnitDirection> directions,
            List<(IChessPiece PossibleTarget, UnitVisibility Visibility)> targets)
        {
            ChessPiece = chessPiece;
            StartingDirection = chessPiece.Direction;
            Start = (chessPiece.XBoard, chessPiece.YBoard);
            End = (xEnd, yEnd);
            Targets = targets;
            Directions = CreateDirectionArrows(directions);
        }

        private List<DirectionArrow> CreateDirectionArrows(IEnumerable<UnitDirection> directions)
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
                Directions.Add(CopyDirectionArrow(arrow));
            }

            return result;
        }

        private DirectionArrow CopyDirectionArrow(DirectionArrow toCopy)
        {
            return new DirectionArrow()
            {
                Move = this,
                Direction = toCopy.Direction,
                Depth = toCopy.Depth,
            };
        }
    }
}