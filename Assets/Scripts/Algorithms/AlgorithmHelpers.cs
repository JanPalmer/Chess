using System.Collections.Generic;
using System.Linq;
using Enums;
using Infrastructure;
using Models;
using UnityEngine;


namespace Algorithms
{
    public static class AlgorithmHelpers
    {
        public static Board CopyBoard(Board boardToCopy)
        {
            var resultBoard = new Board();

            // Add copies of pieces for evaluation
            var boardSizeX = boardToCopy.Positions.GetLength(0);
            var boardSizeY = boardToCopy.Positions.GetLength(1);

            for (int y = 0; y < boardSizeY; y++)
            {
                for (int x = 0; x < boardSizeX; x++)
                {
                    var chessPiece = boardToCopy.GetPosition(x, y);
                    if (chessPiece != null)
                    {
                        if (chessPiece.Role != UnitRole.Wall)
                        {
                            resultBoard.SetPosition(new Chessman(chessPiece, resultBoard), x, y);
                        }
                        else
                        {
                            resultBoard.SetPosition(new Wall() { XBoard = x, YBoard = y, Board = resultBoard }, x, y);
                        }
                    }
                }
            }

            return resultBoard;
        }


        public static List<DirectionArrow> GetAvailableArrows(
            PlayerSide originalPlayer,
            Board board,
            IEnumerable<DirectionArrow> arrowsSoFar)
        {
            var result = new List<DirectionArrow>();

            var lastPlayer = originalPlayer.GetOpposingPlayer();
            if (arrowsSoFar.Count() > 0)
            {
                lastPlayer = arrowsSoFar.Last().Move.ChessPiece.Player;
            }

            List<IChessPiece> sideToEvaluate;
            var opponent = lastPlayer.GetOpposingPlayer();
            sideToEvaluate = board.GetReadyPiecesForPlayer(opponent);
            if (sideToEvaluate == null || sideToEvaluate.Count == 0)
            {
                // Take into account that if the last player has more than one piece to move of their own,
                // and the opposition has no ready pieces, then the last player can chain activations
                var lastPlayerPieces = board.GetReadyPiecesForPlayer(lastPlayer);
                if (lastPlayerPieces == null || lastPlayerPieces.Count == 0)
                {
                    //Debug.Log($"GetAvailableArrows - no pieces for {currentPlayer}");
                    return result;
                }

                sideToEvaluate = lastPlayerPieces;
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

            if(result.Count(x => x.Move.Targets == null) > 0)
            {
                Debug.Log("Error - no targets");
            }

            return result;
        }

        public static void MakeMove(Board board, ref bool isGameOver, DirectionArrow arrow, IChessPiece target = null)
        {
            board.MoveChessPiece(arrow.Move, arrow.Direction);

            if (target != null)
            {
                board.PerformAttack(arrow.Move, target);
            }

            if (board.GetPiecesForPlayer(arrow.Move.ChessPiece.Player.GetOpposingPlayer()).Count == 0)
            {
                isGameOver = true;
            }
        }

        public static void UndoMove(Board board, ref bool isGameOver, DirectionArrow arrow)
        {
            board.UndoMove(arrow.Move);

            if (board.GetPiecesForPlayer(arrow.Move.ChessPiece.Player.GetOpposingPlayer()).Count != 0)
            {
                isGameOver = false;
            }
        }

        public static int Evaluate(Board board, Dictionary<UnitRole, int> pieceValues, IEnumerable<DirectionArrow> arrows, PlayerSide? playerToEvaluateFor = null)
        {
            var result = 0;
            playerToEvaluateFor ??= arrows.Last().Move.ChessPiece.Player.GetOpposingPlayer();
            //playerToEvaluateFor ??= arrows.First().Move.ChessPiece.Player.GetOpposingPlayer();


            foreach (var piece in board.GetAllPieces())
            {
                if (piece.Player == playerToEvaluateFor)
                {
                    result += pieceValues[piece.Role] * piece.Health;
                }
                else
                {
                    result -= pieceValues[piece.Role] * piece.Health;
                }
            }

            //Debug.Log("Evaluate");

            return result;
        }

        public static DirectionArrow TranslateMove(Board boardTo, DirectionArrow arrowToTranslate)
        {
            var pieceToMove = boardTo.GetPosition(arrowToTranslate.Move.Start.X, arrowToTranslate.Move.Start.Y);
            Chessman pieceToAttack = null;
            if (arrowToTranslate.Move.AttackedChessPiece != null)
            {
                pieceToAttack = boardTo.GetPosition(arrowToTranslate.Move.AttackedChessPiece.XBoard, arrowToTranslate.Move.AttackedChessPiece.YBoard) as Chessman;
            }

            List<(IChessPiece target, UnitVisibility visibility)> targets = new List<(IChessPiece target, UnitVisibility visibility)>();
            foreach (var target in arrowToTranslate.Move.Targets)
            {
                if (target.PossibleTarget == null)
                {
                    continue;
                }
                var targetTranslated = boardTo.GetPosition(target.PossibleTarget.XBoard, target.PossibleTarget.YBoard) as Chessman;
                targets.Add((targetTranslated, target.Visibility));
            }

            DirectionArrow translatedBestMove = new DirectionArrow(
                arrowToTranslate.Direction,
                arrowToTranslate.Depth,
                pieceToMove as Chessman,
                arrowToTranslate.Move.Start.X,
                arrowToTranslate.Move.Start.Y,
                arrowToTranslate.Move.End.X,
                arrowToTranslate.Move.End.Y,
                arrowToTranslate.Move.Directions,
                targets,
                pieceToAttack,
                arrowToTranslate.Move.AttackedChessPieceHealthLost
            );

            return translatedBestMove;
        }
    }
}