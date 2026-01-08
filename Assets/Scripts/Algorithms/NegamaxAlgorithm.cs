using System.Collections.Generic;
using System.Linq;
using Enums;
using Infrastructure;
using Models;
using UnityEngine;

namespace Algorithms
{
    // Negamax with Alpha-beta pruning
    public class NegamaxAlgorithm : IAlgorithm
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
            int maxDepth = 4)
        {
            _originalPlayer = player;
            _maxDepth = maxDepth;
            _simulatedBoard = AlgorithmHelpers.CopyBoard(board);
            _bestEvaluation = int.MinValue;
            _bestArrow = null;
            _isGameOver = false;

            Debug.Log("Board:");

            Debug.Log(_simulatedBoard.ToString());

            Debug.Log("Starting NegaMax with alpha-beta pruning");

            NegaMax(new List<DirectionArrow>(), int.MinValue, int.MaxValue);

            Debug.Log("Finished NegaMax with alpha-beta pruning");

            if (_bestArrow == null)
            {
                Debug.Log("No best move found");
                return null;
            }

            DirectionArrow translatedBestMove = AlgorithmHelpers.TranslateMove(board, _bestArrow);

            Debug.Log($"Best Move - {translatedBestMove.Move.ChessPiece.Role} - {translatedBestMove.Move.Start.X}, {translatedBestMove.Move.Start.Y} " +
            $"to {translatedBestMove.Move.End.X}, {translatedBestMove.Move.End.Y} - Attacked piece {translatedBestMove.Move.AttackedChessPiece?.Role}");
            Debug.Log($"Best evaluation - {_bestEvaluation}");

            return translatedBestMove;

            //return null;
        }

        private int NegaMax(List<DirectionArrow> arrowsSoFar, int alpha, int beta)
        {
            //Debug.Log($"Depth: {movesSoFar.Count} - Side: {currentPlayer.ToString()}");
            //Debug.Log("Pieces to evaluate: " + sideToEvaluate.Count());

            if (_isGameOver)
            {
                //Debug.Log($"NegaMax - gameOver: {_isGameOver}");
                return AlgorithmHelpers.Evaluate(_simulatedBoard, _pieceValues, arrowsSoFar, _originalPlayer);
            }

            if (arrowsSoFar.Count >= _maxDepth)
            {
                return QuiscenceSearch(arrowsSoFar, alpha, beta, 1); ;
            }

            var possibleArrows = AlgorithmHelpers.GetAvailableArrows(_originalPlayer, _simulatedBoard, arrowsSoFar);

            //Debug.Log($"NegaMax - Possible moves: {possibleArrows.Count}");

            int nodeCount = 0;

            int bestValueLocal = int.MinValue;

            foreach (var arrow in possibleArrows)
            {
                foreach (var target in arrow.Move.Targets)
                {
                    //MakeMove(arrow, target.PossibleTarget);
                    AlgorithmHelpers.MakeMove(_simulatedBoard, ref _isGameOver, arrow, target.PossibleTarget);
                    arrowsSoFar.Add(arrow);

                    //Debug.Log($"NegaMax - Added move {arrowsSoFar.Count}");

                    int score = -NegaMax(arrowsSoFar, -beta, -alpha);

                    if (score > bestValueLocal)
                    {
                        bestValueLocal = score;
                        if (score > alpha)
                        {
                            alpha = score;
                        }

                        if (arrowsSoFar.Count == 1)
                        {
                            _bestEvaluation = score;
                            _bestArrow = new DirectionArrow(arrowsSoFar.First());
                        }

                        // _bestEvaluation = score; // Copy the move before UndoMove, to keep the attacked piece
                        // _bestArrow = new DirectionArrow(arrowsSoFar.First());
                    }

                    nodeCount++;

                    arrowsSoFar.RemoveAt(arrowsSoFar.Count - 1);
                    AlgorithmHelpers.UndoMove(_simulatedBoard, ref _isGameOver, arrow);
                    //UndoMove(arrow);

                    //Debug.Log($"NegaMax - Undoing move {arrowsSoFar.Count}");

                    if (score >= beta)
                    {
                        return bestValueLocal;
                    }
                }
            }

            Debug.Log($"Nodes checked: " + nodeCount);

            return bestValueLocal;
        }

        private int QuiscenceSearch(List<DirectionArrow> arrowsSoFar, int alpha, int beta, int quiscenceSearchDepth)
        {
            var bestValue = AlgorithmHelpers.Evaluate(_simulatedBoard, _pieceValues, arrowsSoFar, _originalPlayer);

            if (bestValue >= beta || _isGameOver || quiscenceSearchDepth >= QuiscenceSearchMaxDepth)
            {
                return bestValue;
            }
            if (bestValue > alpha)
            {
                alpha = bestValue;
            }

            var possibleArrows = AlgorithmHelpers.GetAvailableArrows(_originalPlayer, _simulatedBoard, arrowsSoFar);

            //Debug.Log($"QuiscenceSearch - Moves to evaluate: {possibleArrows.Count}");

            foreach (var arrow in possibleArrows)
            {
                // Only check moves that could result in danger
                var moves = new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>(arrow.Move.Targets.Where(x => x.PossibleTarget != null));

                foreach (var target in moves)
                {
                    AlgorithmHelpers.MakeMove(_simulatedBoard, ref _isGameOver, arrow, target.PossibleTarget);
                    arrowsSoFar.Add(arrow);

                    int score = -QuiscenceSearch(arrowsSoFar, -beta, -alpha, quiscenceSearchDepth + 1);

                    if (score > bestValue)
                    {
                        bestValue = score;
                    }
                    if (score > alpha)
                    {
                        alpha = score;
                    }

                    arrowsSoFar.RemoveAt(arrowsSoFar.Count - 1);
                    AlgorithmHelpers.UndoMove(_simulatedBoard, ref _isGameOver, arrow);

                    if (score >= beta)
                    {
                        return score;
                    }
                }
            }

            return bestValue;
        }

        // private int Evaluate(IEnumerable<DirectionArrow> arrows)
        // {
        //     var result = 0;
        //     var playerToEvaluateFor = arrows.Last().Move.ChessPiece.Player.GetOpposingPlayer();

        //     foreach (var piece in _simulatedBoard.GetAllPieces())
        //     {
        //         if (piece.Player == playerToEvaluateFor)
        //         {
        //             result += _pieceValues[piece.Role] * piece.Health;
        //         }
        //         else
        //         {
        //             result -= _pieceValues[piece.Role] * piece.Health;
        //         }
        //     }

        //     //Debug.Log("Evaluate");

        //     return result;
        // }



        // private void MakeMove(DirectionArrow arrow, IChessPiece target = null)
        // {
        //     _simulatedBoard.MoveChessPiece(arrow.Move, arrow.Direction);

        //     if (target != null)
        //     {
        //         _simulatedBoard.PerformAttack(arrow.Move, target);
        //     }

        //     if (_simulatedBoard.GetPiecesForPlayer(arrow.Move.ChessPiece.Player.GetOpposingPlayer()).Count == 0)
        //     {
        //         _isGameOver = true;
        //     }
        // }

        // private void UndoMove(DirectionArrow arrow)
        // {
        //     _simulatedBoard.UndoMove(arrow.Move);

        //     if (_simulatedBoard.GetPiecesForPlayer(arrow.Move.ChessPiece.Player.GetOpposingPlayer()).Count != 0)
        //     {
        //         _isGameOver = false;
        //     }
        // }

        // private List<DirectionArrow> GetAvailableArrows(List<DirectionArrow> arrowsSoFar)
        // {
        //     var result = new List<DirectionArrow>();

        //     var lastPlayer = _originalPlayer;
        //     if (arrowsSoFar.Count > 0)
        //     {
        //         lastPlayer = arrowsSoFar.Last().Move.ChessPiece.Player;
        //     }

        //     List<IChessPiece> sideToEvaluate;
        //     var opponent = lastPlayer.GetOpposingPlayer();
        //     sideToEvaluate = _simulatedBoard.GetReadyPiecesForPlayer(opponent);
        //     if (sideToEvaluate == null || sideToEvaluate.Count == 0)
        //     {
        //         // Take into account that if the last player has more than one piece to move of their own,
        //         // and the opposition has no ready pieces, then the last player can chain activations
        //         var lastPlayerPieces = _simulatedBoard.GetReadyPiecesForPlayer(lastPlayer);
        //         if (lastPlayerPieces == null || lastPlayerPieces.Count == 0)
        //         {
        //             //Debug.Log($"GetAvailableArrows - no pieces for {currentPlayer}");
        //             return result;
        //         }

        //         sideToEvaluate = lastPlayerPieces;
        //     }

        //     foreach (var piece in sideToEvaluate)
        //     {
        //         var possibleMoves = piece.GetPossibleMoves();
        //         //Debug.Log($"GetAvailableArrows - moves: {possibleMoves.Count}");
        //         foreach (var move in possibleMoves)
        //         {
        //             //Debug.Log($"GetAvailableArrows - move.Directions: {move.Directions.Count}");
        //             result.AddRange(move.Directions);
        //         }
        //     }

        //     return result;
        // }
    }
}
