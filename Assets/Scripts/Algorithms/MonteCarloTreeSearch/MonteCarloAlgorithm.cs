using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms;
using Enums;
using UnityEngine;
using Models;
using Codice.CM.Client.Differences;

public class MonteCarloAlgorithm : IAlgorithm
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

    private Board _originalBoard;
    private bool _isGameOver;
    private int _maxDepth;
    private static System.Random _numberGenerator = new();

    private int _bestEvaluation;
    private DirectionArrow _bestArrow;

    public DirectionArrow CalculateNextMove(
        PlayerSide player,
        Board board,
        int maxDepth = 5,
        DirectionArrow lastMove = null)
    {
        _originalPlayer = player;
        _maxDepth = maxDepth;
        _originalBoard = AlgorithmHelpers.CopyBoard(board);
        _bestEvaluation = int.MinValue;
        _bestArrow = null;
        _isGameOver = false;

        Debug.Log("Board:");

        Debug.Log(board.ToString());

        Debug.Log("Starting MCTS");

        MCTS();

        Debug.Log("Finished MCTS");

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

    // https://youtu.be/UXW2yZndl7U?si=dFnJnsk-SFnLKz43
    private void MCTS()
    {
        // Create dummy root node
        var rootPiece = _originalBoard.GetReadyPiecesForPlayer(_originalPlayer).First();
        var rootMove = new DirectionArrow(
            rootPiece.Direction,
            0,
            rootPiece as Chessman,
            rootPiece.XBoard,
            rootPiece.YBoard,
            rootPiece.XBoard,
            rootPiece.YBoard,
            new List<DirectionArrow>(),
            new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>() { (null, UnitVisibility.NotVisible) }
        );
        var rootNode = new MonteCarloDirectionArrow(rootMove);

        // Tree creation
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"Iteration: {i + 1}");

            _isGameOver = false;

            var arrowsSoFar = new List<MonteCarloDirectionArrow>()
            {
                rootNode,
            };

            var boardCopy = AlgorithmHelpers.CopyBoard(_originalBoard);

            Selection(arrowsSoFar, boardCopy);
            Expansion(arrowsSoFar, boardCopy);
            var simulatedActionChain = Simulation(arrowsSoFar, boardCopy);
            Backpropagation(arrowsSoFar, boardCopy);
        }

        // Pick best child after the tree is created
        var bestChild = rootNode.Children.First();
        foreach (var child in rootNode.Children)
        {
            if (child.GetUCT() > bestChild.GetUCT())
            {
                bestChild = child;
            }
        }

        _bestArrow = bestChild;
    }

    // Search through the MCTS subtree
    private List<MonteCarloDirectionArrow> Selection(List<MonteCarloDirectionArrow> arrowsSoFar, Board simulatedBoard)
    {
        var currentNode = arrowsSoFar.Last(); // should start with last done move

        while (currentNode.Children != null)
        {
            MonteCarloDirectionArrow chosenChild = currentNode.Children.First();

            foreach (var child in currentNode.Children)
            {
                if (child.GetUCT() > chosenChild.GetUCT())
                {
                    chosenChild = child;
                }
            }

            arrowsSoFar.Add(chosenChild);
            AlgorithmHelpers.MakeMove(simulatedBoard, ref _isGameOver, chosenChild, chosenChild.Target);
            currentNode = chosenChild;
        }

        return arrowsSoFar;
    }

    // Add a new node to the node chosen in Selection step
    private void Expansion(List<MonteCarloDirectionArrow> arrowsSoFar, Board simulatedBoard)
    {
        var currentNode = arrowsSoFar.Last();

        if (currentNode.NumberOfVisits == 0)
        {
            return;
        }

        var availableArrows = AlgorithmHelpers.GetAvailableArrows(_originalPlayer, simulatedBoard, arrowsSoFar);
        currentNode.Children = new List<MonteCarloDirectionArrow>();
        int iterator = 0;
        foreach (var arrow in availableArrows)
        {
            foreach (var target in arrow.Move.Targets)
            {
                var newNode = new MonteCarloDirectionArrow(arrow)
                {
                    Parent = currentNode,
                    Target = target.PossibleTarget,
                    Id = currentNode.Id + " " + (iterator++),
                };
                currentNode.Children.Add(newNode);
            }
        }

        currentNode = currentNode.Children.First(); // choose first child node of a newly expanded node

        arrowsSoFar.Add(currentNode);
        AlgorithmHelpers.MakeMove(simulatedBoard, ref _isGameOver, currentNode, currentNode.Move.Targets.First().PossibleTarget);
    }

    // Simulation
    private List<DirectionArrow> Simulation(List<MonteCarloDirectionArrow> arrowsSoFar, Board simulatedBoard)
    {
        var _simulatedBoard = AlgorithmHelpers.CopyBoard(_originalBoard);
        _isGameOver = false;
        var arrowSoFarExtended = new List<DirectionArrow>(arrowsSoFar); // list to which arrows will be added for simulation

        while (_isGameOver == false && arrowSoFarExtended.Count < _maxDepth)
        {
            var availableArrows = AlgorithmHelpers.GetAvailableArrows(_originalPlayer, simulatedBoard, arrowSoFarExtended);

            var randomArrowIndex = (int)Math.Round(_numberGenerator.NextDouble() * (availableArrows.Count - 1));
            var randomArrow = availableArrows.ElementAt(randomArrowIndex);
            var randomTargetIndex = (int)Math.Round(_numberGenerator.NextDouble() * (randomArrow.Move.Targets.Count - 1));
            var randomTarget = randomArrow.Move.Targets.ElementAt(randomTargetIndex);

            arrowSoFarExtended.Add(randomArrow);
            AlgorithmHelpers.MakeMove(simulatedBoard, ref _isGameOver, randomArrow, randomTarget.PossibleTarget);
        }

        return arrowSoFarExtended;
    }

    // Update all previous/visited nodes of the search tree
    private void Backpropagation(List<MonteCarloDirectionArrow> arrowsSoFar, Board simulatedBoard)
    {
        var valueAtEndNode = AlgorithmHelpers.Evaluate(simulatedBoard, _pieceValues, arrowsSoFar);
        foreach (var arrow in arrowsSoFar)
        {
            arrow.NumberOfVisits += 1;
            arrow.Value += valueAtEndNode;
        }
    }
}
