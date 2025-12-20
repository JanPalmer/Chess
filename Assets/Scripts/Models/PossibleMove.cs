using System.Collections.Generic;
using Enums;

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
        public Chessman AttackedChessPiece = null;
        public int AttackedChessPieceHealthLost = 0;

        private List<(IChessPiece PossibleTarget, UnitVisibility Visibility)> _targets;

        // not readonly, as it's assigned later
        public List<(IChessPiece PossibleTarget, UnitVisibility Visibility)> Targets
        {
            get => _targets;
            set
            {
                // if (value == null || value.Count == 0)
                // {
                //     Debug.Log("Targets: 0");
                // }
                _targets = value;
            }
        }

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
            AttackedChessPiece = moveToCopy.AttackedChessPiece;
            AttackedChessPieceHealthLost = moveToCopy.AttackedChessPieceHealthLost;
            Targets = moveToCopy.Targets != null ? new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>(moveToCopy.Targets) : new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>();
            Directions = CopyDirectionArrows(moveToCopy.Directions);

            // if (Targets == null || Targets.Count == 0)
            // {
            //     Debug.Log("Targets: 0");
            // }
        }

        public PossibleMove(Chessman chessPiece, int xEnd, int yEnd, IEnumerable<UnitDirection> directions)
        {
            ChessPiece = chessPiece;
            StartingDirection = chessPiece.Direction;
            Start.X = chessPiece.XBoard;
            Start.Y = chessPiece.YBoard;
            End.X = xEnd;
            End.Y = yEnd;
            Directions = CreateDirectionArrows(directions);
        }

        public PossibleMove(Chessman chessPiece, int xStart, int yStart, int xEnd, int yEnd, IEnumerable<UnitDirection> directions)
        {
            ChessPiece = chessPiece;
            StartingDirection = chessPiece.Direction;
            Start.X = xStart;
            Start.Y = yStart;
            End.X = xEnd;
            End.Y = yEnd;
            Directions = CreateDirectionArrows(directions);
        }

        public PossibleMove(
            Chessman chessPiece,
            int xStart,
            int yStart,
            int xEnd,
            int yEnd,
            IEnumerable<DirectionArrow> directions,
            List<(IChessPiece PossibleTarget, UnitVisibility Visibility)> targets,
            Chessman attackedChessPiece = null,
            int attackedChessPieceHealthLost = 0)
        {
            ChessPiece = chessPiece;
            StartingDirection = chessPiece.Direction;
            Start.X = xStart;
            Start.Y = yStart;
            End.X = xEnd;
            End.Y = yEnd;
            Targets = targets;
            Directions = CopyDirectionArrows(directions);
            AttackedChessPiece = attackedChessPiece;
            AttackedChessPieceHealthLost = attackedChessPieceHealthLost;
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

        private List<DirectionArrow> CopyDirectionArrows(IEnumerable<DirectionArrow> toCopy)
        {
            var result = new List<DirectionArrow>();

            foreach (var arrow in toCopy)
            {
                result.Add(CopyDirectionArrow(arrow));
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