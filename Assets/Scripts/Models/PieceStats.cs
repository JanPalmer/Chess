using System.Collections.Generic;
using Enums;

namespace Models
{
    public static class PieceStats
    {
        private static Dictionary<UnitRole, (int, int, int, int)> PieceStatistics = new Dictionary<UnitRole, (int, int, int, int)>()
        {
            { UnitRole.Unknown, (0, 0, 0, 0) },
            { UnitRole.Pawn, (3, 1, 3, 2) },
            { UnitRole.Rook, (6, 2, 4, 3) },
        };

        private static void SetStats(IChessPiece piece, (int health, int movement, int firepower, int survivability) stats)
        {
            piece.Health = stats.health;
            piece.Movement = stats.movement;
            piece.Firepower = stats.firepower;
            piece.Survivability = stats.survivability;
        }

        public static void SetPieceStats(this IChessPiece piece)
        {
            var statsToSet = PieceStatistics[UnitRole.Unknown];
            if (PieceStatistics.TryGetValue(piece.Role, out var stats))
            {
                statsToSet = stats;
            }

            SetStats(piece, statsToSet);
        }
    }
}