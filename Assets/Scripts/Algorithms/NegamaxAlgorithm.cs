using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Components;
using Models;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Algorithms
{
    public class NegamaxAlgorithm : IAlgorithm
    {
        private Dictionary<ChessPieceRole, int> _pieceValues = new Dictionary<ChessPieceRole, int>(){
        { ChessPieceRole.Unknown, 0 },
        { ChessPieceRole.Pawn, 1 },
        { ChessPieceRole.Bishop, 3 },
        { ChessPieceRole.Knight, 3 },
        { ChessPieceRole.Rook, 5 },
        { ChessPieceRole.Queen, 8 },
        { ChessPieceRole.King, 1000 },
    };

        private PlayerSide _originalPlayer;
        // private Chessman[,] _board;
        // private List<Chessman> _playerBlack;
        // private List<Chessman> _playerWhite;

        private Board _simulatedBoard;
        private bool _isGameOver;
        private int _maxDepth;

        private int _bestEvaluation;
        private PossibleMove _bestMove;


        public PossibleMove CalculateNextMove(
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
            _bestMove = null;
            _isGameOver = false;

            for (int y = 0; y < boardSizeY; y++)
            {
                for (int x = 0; x < boardSizeX; x++)
                {
                    var chessPiece = board.GetPosition(x, y);
                    if (chessPiece != null)
                    {
                        _simulatedBoard.SetPosition(new Chessman(chessPiece, _simulatedBoard), x, y);
                    }
                }
            }

            Debug.Log("Starting NegaMax");

            NegaMax(new List<PossibleMove>());

            if (_bestMove == null)
            {
                Debug.Log("No best move found");
            }

            //Debug.Log($"Best Move - ");

            var pieceToMove = board.GetPosition(_bestMove.Start.X, _bestMove.Start.Y);
            var pieceToRemove = board.GetPosition(_bestMove.End.X, _bestMove.End.Y);


            PossibleMove translatedBestMove;
            if (pieceToRemove != null)
            {
                Debug.Log($"Best Move - {_bestMove.ChessPiece.Role} - {_bestMove.Start.X}, {_bestMove.Start.Y} to {_bestMove.End.X}, {_bestMove.End.Y} - Removed piece {_bestMove.RemovedChessPiece.Role}");
                translatedBestMove = new PossibleMove(pieceToMove, pieceToRemove);
            }
            else
            {
                Debug.Log($"Best Move - {_bestMove.ChessPiece.Role} - {_bestMove.Start.X}, {_bestMove.Start.Y} to {_bestMove.End.X}, {_bestMove.End.Y}");
                translatedBestMove = new PossibleMove(pieceToMove, _bestMove.End.X, _bestMove.End.Y);
            }

            return translatedBestMove;
        }

        private void NegaMax(List<PossibleMove> movesSoFar)
        {
            var currentPlayer = (PlayerSide)(((int)_originalPlayer + movesSoFar.Count) % 2);

            if (movesSoFar.Count >= _maxDepth || _isGameOver)
            {
                var evaluationResult = Evaluate(movesSoFar);
                if (evaluationResult > _bestEvaluation)
                {
                    //Debug.Log($"Evaluation: {evaluationResult}");
                    _bestEvaluation = evaluationResult;
                    _bestMove = new PossibleMove(movesSoFar.First());
                }

                return;
            }

            //Debug.Log($"Depth: {movesSoFar.Count} - Side: {currentPlayer.ToString()}");

            var listOfChesspieces = _simulatedBoard.Positions.Cast<Chessman>();
            var sideToEvaluate = listOfChesspieces.Where(x => x != null && x.Player == currentPlayer && x.IsRemoved == false).ToList();

            if (sideToEvaluate == null)
            {
                //Debug.Log("Nothing to evaluate");
                return;
            }

            //Debug.Log("Pieces to evaluate: " + sideToEvaluate.Count());

            foreach (var piece in sideToEvaluate)
            {
                var possibleMoves = piece.GetPossibleMoves();

                //Debug.Log($"Evaluating piece {piece.Role.ToString()} - possible moves: {possibleMoves.Count}");

                foreach (var move in possibleMoves)
                {
                    MakeMove(move);
                    movesSoFar.Add(move);

                    //Debug.Log($"Added move {movesSoFar.Count}");

                    NegaMax(movesSoFar);

                    //if (_bestMove != null) return;

                    movesSoFar.RemoveAt(movesSoFar.Count - 1);
                    UndoMove(move);

                    //Debug.Log($"Undoing move {movesSoFar.Count}");
                }
            }
        }

        private int Evaluate(IEnumerable<PossibleMove> moves)
        {
            var result = 0;

            foreach (var move in moves)
            {
                var chesspieceRole = ChessPieceRole.Unknown;
                if (move.RemovedChessPiece != null)
                {
                    chesspieceRole = move.RemovedChessPiece.Role;
                }

                var modifier = (move.ChessPiece.Player == _originalPlayer) ? 1 : -1;
                result += modifier * _pieceValues[chesspieceRole];
            }

            return result;
        }


        private void MakeMove(PossibleMove move)
        {
            if (move.RemovedChessPiece != null && move.RemovedChessPiece.Role == ChessPieceRole.King)
            {
                _isGameOver = true;
            }

            _simulatedBoard.MoveChessPiece(move);
        }

        private void UndoMove(PossibleMove move)
        {
            if (move.RemovedChessPiece != null && move.RemovedChessPiece.Role == ChessPieceRole.King)
            {
                _isGameOver = false;
            }

            _simulatedBoard.UndoMove(move);
        }

        // private List<PossibleMove> GetAvailableMoves(IEnumerable<Chessman> chessPieces)
        // {
        //     var result = new List<PossibleMove>();

        //     foreach (var piece in chessPieces)
        //     {
        //         var possibleMoves = piece.GetPossibleMoves();
        //         result.AddRange(possibleMoves);
        //     }

        //     return result;
        // }
    }
}
