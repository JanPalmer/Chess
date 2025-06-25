using System;
using System.Collections.Generic;
using System.Linq;
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

        private Board _board;
        private bool _isGameOver = false;
        private int _maxDepth;

        private int _bestEvaluation = int.MinValue;
        private PossibleMove? _bestMove = null;


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
            _board = new Board();
            //_board = new Chessman[boardSizeX, boardSizeY];
            for (int y = 0; y < boardSizeY; y++)
            {
                for (int x = 0; x < boardSizeX; x++)
                {
                    var chessPiece = board.GetPosition(x, y);
                    if (chessPiece != null)
                    {
                        _board.SetPosition(new Chessman(chessPiece), x, y);
                        //_board[x, y] = new Chessman(chessPiece);
                    }
                }
            }

            // _board.PlayerBlack = (from Chessman piece in _board where piece.Player == PlayerSide.Black select piece).ToList();
            // _playerWhite = (from Chessman piece in _board where piece.Player == PlayerSide.White select piece).ToList();

            //var result = 

            // _playerBlack = new List<Chessman>();
            // foreach (var piece in board.PlayerBlack)
            // {
            //     _playerBlack.Add(new Chessman(piece.GetComponent<Chessman>()));
            // }

            // _playerWhite = new List<Chessman>();
            // foreach (var piece in playerWhite)
            // {
            //     _playerWhite.Add(new Chessman(piece.GetComponent<Chessman>()));
            // }

            NegaMax(new List<PossibleMove>());

            return _bestMove!;
        }

        private void NegaMax(List<PossibleMove> movesSoFar)
        {
            var currentPlayer = (PlayerSide)(((int)_originalPlayer + movesSoFar.Count) % 2);

            if (movesSoFar.Count >= _maxDepth || _isGameOver)
            {
                var evaluationResult = Evaluate(movesSoFar);
                if (evaluationResult > _bestEvaluation)
                {
                    Debug.Log($"Evaluation: {evaluationResult}");
                    _bestEvaluation = evaluationResult;
                    _bestMove = movesSoFar.First();
                }
            }

            //var max = int.MinValue;

            var sideToEvaluate = _board.Positions.Cast<Chessman>().Where(x => x.Player == currentPlayer);

            foreach (var piece in sideToEvaluate)
            {
                var possibleMoves = piece.GetPossibleMoves();

                foreach (var move in possibleMoves)
                {
                    MakeMove(move);
                    movesSoFar.Add(move);

                    Debug.Log(movesSoFar.Count);

                    System.Threading.Thread.Sleep(50);

                    NegaMax(movesSoFar);

                    movesSoFar.RemoveAt(movesSoFar.Count - 1);
                    UndoMove(move);
                }
            }
        }

        private int Evaluate(IEnumerable<PossibleMove> moves)
        {
            var result = 0;

            foreach (var move in moves)
            {
                var chesspieceRole = move.RemovedChessPiece?.Role ?? ChessPieceRole.Unknown;
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

            _board.SetPosition(move.ChessPiece, move.End.X, move.End.Y);
        }

        private void UndoMove(PossibleMove move)
        {
            if (move.RemovedChessPiece != null && move.RemovedChessPiece.Role == ChessPieceRole.King)
            {
                _isGameOver = true;
            }

            move.ChessPiece.XBoard = move.Start.X;
            move.ChessPiece.YBoard = move.Start.Y;
            _board.SetPosition(move.RemovedChessPiece, move.End.X, move.End.Y);
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
