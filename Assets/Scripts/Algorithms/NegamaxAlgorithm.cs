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

            Debug.Log("Starting NegaMax");

            NegaMax(new List<DirectionArrow>(), int.MinValue, int.MaxValue);

            Debug.Log("Finished NegaMax");

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

        private int NegaMax(List<DirectionArrow> arrowsSoFar, int alpha, int beta)
        {
            //Debug.Log($"Depth: {movesSoFar.Count} - Side: {currentPlayer.ToString()}");
            //Debug.Log("Pieces to evaluate: " + sideToEvaluate.Count());

            if (_isGameOver)
            {
                Debug.Log($"NegaMax - gameOver: {_isGameOver}");
                return Evaluate(arrowsSoFar);
            }

            if (arrowsSoFar.Count >= _maxDepth)
            {
                return QuiscenceSearch(arrowsSoFar, alpha, beta, 1); ;
            }

            var possibleArrows = GetAvailableArrows(arrowsSoFar);

            Debug.Log($"NegaMax - Possible moves: {possibleArrows.Count}");

            int bestValueLocal = int.MinValue;

            foreach (var arrow in possibleArrows)
            {
                var moves = new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>(arrow.Move.Targets)
                {
                    (null, UnitVisibility.NotVisible)
                };

                foreach (var target in moves)
                {
                    MakeMove(arrow, target.PossibleTarget);
                    arrowsSoFar.Add(arrow);

                    Debug.Log($"NegaMax - Added move {arrowsSoFar.Count}");

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

                    arrowsSoFar.RemoveAt(arrowsSoFar.Count - 1);
                    UndoMove(arrow);

                    Debug.Log($"NegaMax - Undoing move {arrowsSoFar.Count}");

                    if (score >= beta)
                    {
                        return bestValueLocal;
                    }
                }
            }

            return bestValueLocal;
        }

        private int QuiscenceSearch(List<DirectionArrow> arrowsSoFar, int alpha, int beta, int quiscenceSearchDepth)
        {
            var bestValue = Evaluate(arrowsSoFar);

            if (bestValue >= beta || _isGameOver || quiscenceSearchDepth >= QuiscenceSearchMaxDepth)
            {
                return bestValue;
            }
            if (bestValue > alpha)
            {
                alpha = bestValue;
            }

            var possibleArrows = GetAvailableArrows(arrowsSoFar);

            Debug.Log($"QuiscenceSearch - Moves to evaluate: {possibleArrows.Count}");

            foreach (var arrow in possibleArrows)
            {
                var moves = new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>(arrow.Move.Targets)
                {
                    (null, UnitVisibility.NotVisible)
                };

                foreach (var target in moves)
                {
                    MakeMove(arrow, target.PossibleTarget);
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
                    UndoMove(arrow);

                    if (score >= beta)
                    {
                        return score;
                    }
                }
            }

            return bestValue;
        }

        private int Evaluate(IEnumerable<DirectionArrow> arrows)
        {
            var result = 0;

            // foreach (var arrow in arrows)
            // {
            //     var chesspieceRole = UnitRole.Unknown;
            //     if (arrow.Move.AttackedChessPiece != null)
            //     {
            //         chesspieceRole = arrow.Move.AttackedChessPiece.Role;
            //     }

            //     result += (arrow.Move.ChessPiece.Player == _originalPlayer) ? _pieceValues[chesspieceRole] : -_pieceValues[chesspieceRole];
            // }

            // var lastArrow = arrows.Last();
            // var chesspieceRole = UnitRole.Unknown;
            // if (lastArrow.Move.AttackedChessPiece != null)
            // {
            //     chesspieceRole = lastArrow.Move.AttackedChessPiece.Role;
            // }

            // result += (lastArrow.Move.ChessPiece.Player == _originalPlayer) ? _pieceValues[chesspieceRole] : -_pieceValues[chesspieceRole];

            foreach (var piece in _simulatedBoard.GetAllPieces())
            {
                if (piece.Player == arrows.Last().Move.ChessPiece.Player)
                {
                    result -= _pieceValues[piece.Role] * piece.Health;
                }
                else
                {
                    result += _pieceValues[piece.Role] * piece.Health;
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
