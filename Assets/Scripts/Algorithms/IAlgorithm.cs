using Enums;
using Models;

namespace Algorithms
{
    public interface IAlgorithm
    {
        public DirectionArrow CalculateNextMove(
            PlayerSide player,
            Board board,
            int maxDepth,
            DirectionArrow lastMove = null);
    }
}
