using System.Collections.Generic;
using System.Linq;
using Enums;
using Infrastructure;
using Models;
using UnityEngine;

namespace Algorithms
{
    // Negamax without Alpha-Beta pruning
    public class NegamaxBasicAlgorithm : IAlgorithm
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

        private PlayerSide _originalPlayer;
        // private Chessman[,] _board;
        // private List<Chessman> _playerBlack;
        // private List<Chessman> _playerWhite;

        private Board _simulatedBoard;
        private bool _isGameOver;
        private int _maxDepth;

        private int _bestEvaluation;
        private DirectionArrow _bestArrow;

        private int _nodesTotal = 0;


        public DirectionArrow CalculateNextMove(
            PlayerSide player,
            Board board,
            int maxDepth = 2,
            DirectionArrow lastMove = null)
        {
            _originalPlayer = player;
            _maxDepth = maxDepth;

            _simulatedBoard = AlgorithmHelpers.CopyBoard(board);
            _bestEvaluation = int.MinValue;
            _bestArrow = null;
            _isGameOver = false;

            Debug.Log("Board:");

            Debug.Log(_simulatedBoard.ToString());

            Debug.Log("Starting NegaMax (basic)");

            NegaMax(new List<DirectionArrow>());

            Debug.Log("Finished NegaMax (basic)");

            if (_bestArrow == null)
            {
                Debug.Log("No best move found");
                return null;
            }

            DirectionArrow translatedBestMove = AlgorithmHelpers.TranslateMove(board, _bestArrow);

            Debug.Log($"Best Move - {translatedBestMove.Move.ChessPiece.Role} - {translatedBestMove.Move.Start.X}, {translatedBestMove.Move.Start.Y} " +
            $"to {translatedBestMove.Move.End.X}, {translatedBestMove.Move.End.Y} - Attacked piece {translatedBestMove.Move.AttackedChessPiece?.Role}");
            Debug.Log($"Best evaluation - {_bestEvaluation}");
            Debug.Log($"Nodes searched: {_nodesTotal}");

            return translatedBestMove;

            //return null;
        }

        private void NegaMax(List<DirectionArrow> arrowsSoFar)
        {
            //var currentPlayer = arrowsSoFar.Last().Move.ChessPiece.Player.GetOpposingPlayer();

            if (arrowsSoFar.Count >= _maxDepth || _isGameOver)
            {
                var evaluationResult = AlgorithmHelpers.Evaluate(_simulatedBoard, _pieceValues, arrowsSoFar);
                if (evaluationResult > _bestEvaluation)
                {
                    //Debug.Log($"Evaluation: {evaluationResult}");
                    _bestEvaluation = evaluationResult;
                    _bestArrow = new DirectionArrow(arrowsSoFar.First());
                }

                return;
            }
            var possibleArrows = AlgorithmHelpers.GetAvailableArrows(_originalPlayer, _simulatedBoard, arrowsSoFar);

            _nodesTotal += possibleArrows.Count;

            //Debug.Log($"NegaMax - Possible moves: {possibleArrows.Count}");

            //Debug.Log("Pieces to evaluate: " + sideToEvaluate.Count());

            foreach (var arrow in possibleArrows)
            {
                foreach (var target in arrow.Move.Targets)
                {
                    AlgorithmHelpers.MakeMove(_simulatedBoard, ref _isGameOver, arrow, target.PossibleTarget);
                    arrowsSoFar.Add(arrow);

                    //Debug.Log($"Added move {movesSoFar.Count}");

                    NegaMax(arrowsSoFar);

                    //if (_bestMove != null) return;

                    arrowsSoFar.RemoveAt(arrowsSoFar.Count - 1);
                    AlgorithmHelpers.UndoMove(_simulatedBoard, ref _isGameOver, arrow);

                    //Debug.Log($"Undoing move {movesSoFar.Count}");
                }
            }
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
