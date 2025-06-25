using Models;
using UnityEngine;

namespace Algorithms
{
    public interface IAlgorithm
    {
        public PossibleMove CalculateNextMove(
            PlayerSide player,
            Board board,
            int maxDepth);
    }
}
