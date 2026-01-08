using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Models;

namespace Algorithms
{
    public class NegaScoutAlgorithm : IAlgorithm
    {
        private Dictionary<UnitRole, int> _pieceValues = new Dictionary<UnitRole, int>()
        {
            { UnitRole.Unknown, 0 },
            { UnitRole.Pawn, 2 },
            { UnitRole.Bishop, 3 },
            { UnitRole.Knight, 3 },
            { UnitRole.Rook, 5 },
            { UnitRole.Queen, 8 },
            { UnitRole.King, 1000 },
        };

        private const int QuiscenceSearchMaxDepth = 1;

        private PlayerSide _originalPlayer;
        // private Chessman[,] _board;
        // private List<Chessman> _playerBlack;
        // private List<Chessman> _playerWhite;

        private Board _simulatedBoard;
        private bool _isGameOver;
        private int _maxDepth;

        private int _bestEvaluation;
        private DirectionArrow _bestArrow;

        public DirectionArrow CalculateNextMove(
            PlayerSide player,
            Board board,
            int maxDepth = 3)
        {
            _originalPlayer = player;
            _maxDepth = maxDepth;
            _simulatedBoard = AlgorithmHelpers.CopyBoard(board);
            _bestEvaluation = int.MinValue;
            _bestArrow = null;
            _isGameOver = false;

            Debug.Log("Board:");

            Debug.Log(_simulatedBoard.ToString());

            Debug.Log("Starting NegaScout");

            NegaScout(new List<DirectionArrow>(), int.MinValue, int.MaxValue);

            Debug.Log("Finished NegaScout");

            if (_bestArrow == null)
            {
                Debug.Log("No best move found");
                return null;
            }

            DirectionArrow translatedBestMove = AlgorithmHelpers.TranslateMove(board, _bestArrow);

            var pieceToAttack = translatedBestMove.Move.AttackedChessPiece?.Role.ToString() ?? "Nothing";
            Debug.Log($"Best Move - {translatedBestMove.Move.ChessPiece.Role} - {translatedBestMove.Move.Start.X}, {translatedBestMove.Move.Start.Y} " +
            $"to {translatedBestMove.Move.End.X}, {translatedBestMove.Move.End.Y} - Attacked piece {pieceToAttack}");
            Debug.Log($"Best evaluation - {_bestEvaluation}");

            return translatedBestMove;

            //return null;
        }

        private int NegaScout(List<DirectionArrow> arrowsSoFar, int alpha, int beta)
        {
            //Debug.Log($"Depth: {movesSoFar.Count} - Side: {currentPlayer.ToString()}");
            //Debug.Log("Pieces to evaluate: " + sideToEvaluate.Count());

            if (_isGameOver || arrowsSoFar.Count >= _maxDepth)
            {
                Debug.Log($"NegaScout - gameOver: {_isGameOver}");
                return AlgorithmHelpers.Evaluate(_simulatedBoard, _pieceValues, arrowsSoFar);
            }

            // if (arrowsSoFar.Count >= _maxDepth)
            // {
            //     return QuiscenceSearch(arrowsSoFar, alpha, beta, 1); ;
            // }

            var possibleArrows = AlgorithmHelpers.GetAvailableArrows(_originalPlayer, _simulatedBoard, arrowsSoFar);

            Debug.Log($"NegaScout - Possible moves: {possibleArrows.Count}");

            int bestValueLocal = int.MinValue;
            int b = beta;

            foreach (var arrow in possibleArrows)
            {
                foreach (var target in arrow.Move.Targets)
                {
                    AlgorithmHelpers.MakeMove(_simulatedBoard, ref _isGameOver, arrow, target.PossibleTarget);
                    arrowsSoFar.Add(arrow);

                    Debug.Log($"NegaScout - Added move {arrowsSoFar.Count}");

                    int score = -NegaScout(arrowsSoFar, -b, -alpha);

                    if (score > alpha && score < beta && arrowsSoFar.Count > 1) // re-search
                    {
                        score = -NegaScout(arrowsSoFar, -beta, -alpha);
                    }

                    if (score > alpha)
                    {
                        alpha = score;

                        if (score > bestValueLocal && arrowsSoFar.Count == 1)
                        {
                            bestValueLocal = alpha;

                            _bestEvaluation = alpha;
                            _bestArrow = new DirectionArrow(arrowsSoFar.First());
                        }
                    }

                    arrowsSoFar.RemoveAt(arrowsSoFar.Count - 1);
                    AlgorithmHelpers.UndoMove(_simulatedBoard, ref _isGameOver, arrow);

                    Debug.Log($"NegaScout - Undoing move {arrowsSoFar.Count}");

                    if (alpha >= beta)
                    {
                        return alpha;
                    }

                    b = alpha + 1;
                }
            }

            return alpha;
        }
    }
}
