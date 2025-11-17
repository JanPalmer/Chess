using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Algorithms;
using Models;
using Enums;
using Infrastructure;

public class MonteCarloTests
{
    private Board _simulatedBoard;
    private IAlgorithm _algorithm;
    private const int TreeSearchDepth = 3;

    [SetUp]
    public void SetUpTests()
    {
        _algorithm = new MonteCarloAlgorithm();
    }

    [TearDown]
    public void TearDownTests()
    {

    }

    [Test]
    public void NegaMaxTests_ReturnsMove()
    {
        _simulatedBoard = TestHelpers.InitializeBoard_1v3();

        var currentPlayer = PlayerSide.Blue;
        var depth = 3;

        var nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

        Assert.IsNotNull(nextArrow);
    }

    [Test]
    public void NegaMaxTests_SelectsOtherPlayerWhenOneSideRunsOutOfMoves()
    {
        // Tests situation, where one player (Blue) has more units than the other (Orange)
        // Since the game is based on turns and activations, and in a turn all units must be activated before the turn ends,
        // the algorithm should pick moves for the Blue player until there are no more units to activate, then switch to the other player
        // once the turn ends

        _simulatedBoard = TestHelpers.InitializeBoard_1v3();

        var currentPlayer = PlayerSide.Blue; // set Blue as "last player to move", so the algorithm picks a move for Orange and moves its only piece
        var depth = 2;

        // Move Orange
        var nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

        Assert.AreEqual(nextArrow.Move.ChessPiece.Player, PlayerSide.Orange);

        // Move 3 Blue pieces in a sequence
        TestHelpers.PerformMove(_simulatedBoard, nextArrow);
        //AlgorithmHelpers.MakeMove(_simulatedBoard, ref nextArrow);
        currentPlayer = nextArrow.Move.ChessPiece.Player;
        nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

        Assert.AreEqual(nextArrow.Move.ChessPiece.Player, PlayerSide.Blue);

        TestHelpers.PerformMove(_simulatedBoard, nextArrow);
        currentPlayer = nextArrow.Move.ChessPiece.Player;
        nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

        Assert.AreEqual(nextArrow.Move.ChessPiece.Player, PlayerSide.Blue);

        TestHelpers.PerformMove(_simulatedBoard, nextArrow);
        currentPlayer = nextArrow.Move.ChessPiece.Player;
        nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

        Assert.AreEqual(nextArrow.Move.ChessPiece.Player, PlayerSide.Blue);

        // Should start another turn and pick Orange again
        TestHelpers.PerformMove(_simulatedBoard, nextArrow);
        currentPlayer = nextArrow.Move.ChessPiece.Player;
        nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

        Assert.AreEqual(nextArrow.Move.ChessPiece.Player, PlayerSide.Orange);
    }

    [Test]
    public void NegaMaxTests_SelectsOtherPlayerUntilTurnEnds()
    {
        _simulatedBoard = TestHelpers.InitializeBoard_3v3();

        var startingPlayer = PlayerSide.Blue;
        var currentPlayer = PlayerSide.Blue; // set Blue as "last player to move", so the algorithm picks a move for Orange and moves its only piece
        var depth = 2;

        // 7 - to move through all of one turn and start the second one
        for (int i = 0; i < 7; i++)
        {
            var nextArrow = _algorithm.CalculateNextMove(currentPlayer, _simulatedBoard, depth);

            var playerThatShouldBeMoved = i % 2 == 0 ? startingPlayer.GetOpposingPlayer() : startingPlayer;
            Assert.AreEqual(nextArrow.Move.ChessPiece.Player, playerThatShouldBeMoved);

            TestHelpers.PerformMove(_simulatedBoard, nextArrow);
            currentPlayer = nextArrow.Move.ChessPiece.Player;
        }
    }
}
