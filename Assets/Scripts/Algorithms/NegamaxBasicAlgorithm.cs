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
            int maxDepth = 2)
        {
            _originalPlayer = player;
            _maxDepth = maxDepth;

            // Add copies of pieces for evaluation
            var boardSizeX = board.Positions.GetLength(0);
            var boardSizeY = board.Positions.GetLength(1);
            _simulatedBoard = new Board();
            _bestEvaluation = int.MinValue;
            _bestArrow = null;
            _isGameOver = false;

            for (int y = 0; y < boardSizeY; y++)
            {
                for (int x = 0; x < boardSizeX; x++)
                {
                    var chessPiece = board.GetPosition(x, y);
                    if (chessPiece != null)
                    {
                        if (chessPiece.Role != UnitRole.Wall)
                        {
                            _simulatedBoard.SetPosition(new Chessman(chessPiece, _simulatedBoard), x, y);
                        }
                        else
                        {
                            _simulatedBoard.SetPosition(new Wall() { XBoard = x, YBoard = y, Board = _simulatedBoard }, x, y);
                        }
                    }
                }
            }
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

            var pieceToMove = board.GetPosition(_bestArrow.Move.Start.X, _bestArrow.Move.Start.Y);
            Chessman pieceToAttack = null;
            if (_bestArrow.Move.AttackedChessPiece != null)
            {
                pieceToAttack = board.GetPosition(_bestArrow.Move.AttackedChessPiece.XBoard, _bestArrow.Move.AttackedChessPiece.YBoard) as Chessman;
            }


            List<(IChessPiece target, UnitVisibility visibility)> targets = new List<(IChessPiece target, UnitVisibility visibility)>();
            foreach (var target in _bestArrow.Move.Targets)
            {
                var targetTranslated = board.GetPosition(target.PossibleTarget.XBoard, target.PossibleTarget.YBoard) as Chessman;
                targets.Add((targetTranslated, target.Visibility));
            }

            DirectionArrow translatedBestMove = new DirectionArrow(
                _bestArrow.Direction,
                _bestArrow.Depth,
                pieceToMove as Chessman,
                _bestArrow.Move.Start.X,
                _bestArrow.Move.Start.Y,
                _bestArrow.Move.End.X,
                _bestArrow.Move.End.Y,
                _bestArrow.Move.Directions,
                targets,
                pieceToAttack,
                _bestArrow.Move.AttackedChessPieceHealthLost
            );

            Debug.Log($"Best Move - {translatedBestMove.Move.ChessPiece.Role} - {translatedBestMove.Move.Start.X}, {translatedBestMove.Move.Start.Y} " +
            $"to {translatedBestMove.Move.End.X}, {translatedBestMove.Move.End.Y} - Attacked piece {translatedBestMove.Move.AttackedChessPiece?.Role}");
            Debug.Log($"Best evaluation - {_bestEvaluation}");

            return translatedBestMove;

            //return null;
        }

        private void NegaMax(List<DirectionArrow> arrowsSoFar)
        {
            var currentPlayer = arrowsSoFar.Last().Move.ChessPiece.Player.GetOpposingPlayer();

            if (arrowsSoFar.Count >= _maxDepth || _isGameOver)
            {
                var evaluationResult = Evaluate(arrowsSoFar);
                if (evaluationResult > _bestEvaluation)
                {
                    //Debug.Log($"Evaluation: {evaluationResult}");
                    _bestEvaluation = evaluationResult;
                    _bestArrow = new DirectionArrow(arrowsSoFar.First());
                }

                return;
            }
            var possibleArrows = GetAvailableArrows(arrowsSoFar);

            Debug.Log($"NegaMax - Possible moves: {possibleArrows.Count}");

            //Debug.Log("Pieces to evaluate: " + sideToEvaluate.Count());

            foreach (var arrow in possibleArrows)
            {
                var moves = new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>(arrow.Move.Targets)
                {
                    (null, UnitVisibility.NotVisible)
                };

                //Debug.Log($"Evaluating piece {piece.Role.ToString()} - possible moves: {possibleMoves.Count}");

                foreach (var target in moves)
                {
                    MakeMove(arrow, target.PossibleTarget);
                    arrowsSoFar.Add(arrow);

                    //Debug.Log($"Added move {movesSoFar.Count}");

                    NegaMax(arrowsSoFar);

                    //if (_bestMove != null) return;

                    arrowsSoFar.RemoveAt(arrowsSoFar.Count - 1);
                    UndoMove(arrow);

                    //Debug.Log($"Undoing move {movesSoFar.Count}");
                }
            }
        }

        private int Evaluate(IEnumerable<DirectionArrow> arrows)
        {
            var result = 0;
            var playerToEvaluateFor = arrows.Last().Move.ChessPiece.Player.GetOpposingPlayer();

            foreach (var piece in _simulatedBoard.GetAllPieces())
            {
                if (piece.Player == playerToEvaluateFor)
                {
                    result += _pieceValues[piece.Role] * piece.Health;
                }
                else
                {
                    result -= _pieceValues[piece.Role] * piece.Health;
                }
            }

            //Debug.Log("Evaluate");

            return result;
        }


        private void MakeMove(DirectionArrow arrow, IChessPiece target = null)
        {
            _simulatedBoard.MoveChessPiece(arrow.Move, arrow.Direction);

            if (target != null)
            {
                _simulatedBoard.PerformAttack(arrow.Move, target);
            }

            if (_simulatedBoard.GetPiecesForPlayer(arrow.Move.ChessPiece.Player.GetOpposingPlayer()).Count == 0)
            {
                _isGameOver = true;
            }
        }

        private void UndoMove(DirectionArrow arrow)
        {
            _simulatedBoard.UndoMove(arrow.Move);

            if (_simulatedBoard.GetPiecesForPlayer(arrow.Move.ChessPiece.Player.GetOpposingPlayer()).Count != 0)
            {
                _isGameOver = false;
            }
        }

        private List<DirectionArrow> GetAvailableArrows(List<DirectionArrow> arrowsSoFar)
        {
            var result = new List<DirectionArrow>();

            var currentPlayer = (PlayerSide)(((int)_originalPlayer + arrowsSoFar.Count - 1) % 2) + 1;
            var sideToEvaluate = _simulatedBoard.GetPiecesForPlayer(currentPlayer);
            if (sideToEvaluate == null || sideToEvaluate.Count == 0)
            {
                //Debug.Log($"GetAvailableArrows - no pieces for {currentPlayer}");
                return result;
            }

            foreach (var piece in sideToEvaluate)
            {
                var possibleMoves = piece.GetPossibleMoves();
                //Debug.Log($"GetAvailableArrows - moves: {possibleMoves.Count}");
                foreach (var move in possibleMoves)
                {
                    //Debug.Log($"GetAvailableArrows - move.Directions: {move.Directions.Count}");
                    result.AddRange(move.Directions);
                }
            }

            return result;
        }
    }
}
