using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Components;
using Enums;
using Models;
using Mono.Cecil;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Analytics;

[CreateAssetMenu(fileName = "Board", menuName = "Scriptable Objects/Board")]
public class Board
{
    public IChessPiece[,] Positions { get; set; } = new IChessPiece[8, 8];

    // more of lookup tables to speed up searching, instead of looking through all fields in Positions
    // doesn't contain Walls and Unknowns
    private List<IChessPiece> Pieces { get; } = new List<IChessPiece>(32);

    private void TryClearPieces(int x, int y)
    {
        // Remove piece at current position
        var possiblePiece = GetPosition(x, y);
        if (possiblePiece != null)
        {
            Pieces.Remove(possiblePiece);
        }
    }

    private void TryAddPieces(IChessPiece piece)
    {
        if (!Pieces.Contains(piece)
            && (piece.Role != UnitRole.Wall || piece.Role != UnitRole.Unknown))
        {
            Pieces.Add(piece);
        }
    }

    public void SetPosition(IChessPiece chesspiece, int x, int y)
    {
        TryClearPieces(x, y);
        TryAddPieces(chesspiece);

        chesspiece.XBoard = x;
        chesspiece.YBoard = y;

        Positions[chesspiece.XBoard, chesspiece.YBoard] = chesspiece;
    }

    public void SetPosition(GameObject obj, int x, int y)
    {
        var chesspiece = obj.GetComponent<ChessmanComponent>().PieceInfo;
        SetPosition(chesspiece, x, y);
    }

    public void SetPositionEmpty(int x, int y)
    {
        TryClearPieces(x, y);

        Positions[x, y] = null;
    }

    public IChessPiece GetPosition(int x, int y)
    {
        return Positions[x, y];
    }

    public bool IsPositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Positions.GetLength(0) || y >= Positions.GetLength(1))
        {
            return false;
        }

        return true;
    }

    public Chessman MoveChessPiece(PossibleMove move, UnitDirection direction)
    {
        //Debug.Log($"Move - {move.Start.X}, {move.Start.Y} -> {move.End.X}, {move.End.Y}");

        SetPosition(move.ChessPiece, move.End.X, move.End.Y);
        SetPositionEmpty(move.Start.X, move.Start.Y);

        if (move.RemovedChessPiece != null)
        {
            move.RemovedChessPiece.IsRemoved = true;
            //Debug.Log($"Removed {move.RemovedChessPiece.Role}");
        }

        move.ChessPiece.Direction = direction;

        return move.RemovedChessPiece;
    }

    public Chessman UndoMove(PossibleMove move)
    {
        //Debug.Log($"Undoing move - {move.Start.X}, {move.Start.Y} -> {move.End.X}, {move.End.Y}");

        SetPosition(move.ChessPiece, move.Start.X, move.Start.Y);
        SetPositionEmpty(move.End.X, move.End.Y);

        if (move.RemovedChessPiece != null)
        {
            move.RemovedChessPiece.IsRemoved = false;
            SetPosition(move.RemovedChessPiece, move.End.X, move.End.Y);
        }

        move.ChessPiece.Direction = move.StartingDirection;

        return move.RemovedChessPiece;
    }

    public void GetPossibleTargets(List<PossibleMove> possibleMoves)
    {
        foreach (var move in possibleMoves)
        {
            GetPossibleTargets(move);
        }
    }

    public void GetPossibleTargets(PossibleMove possibleMove)
    {
        var attacker = possibleMove.ChessPiece;
        var possibleTargets = Pieces.Where(x => x.Player != attacker.Player).ToList();
        var result = new List<(IChessPiece PossibleTarget, UnitVisibility Visibility)>();

        foreach (var target in possibleTargets)
        {
            Debug.Log($"Checking visibility: Start: {possibleMove.End.X},{possibleMove.End.Y} - target {target.Role}, {target.XBoard}, {target.YBoard}");

            // Calculate visibility from the end of the move, so where the piece would end up
            var visibility = DetermineLineOfSight(possibleMove.End, possibleMove.ChessPiece, target);
            if (visibility != UnitVisibility.NotVisible)
            {
                result.Add((target, visibility));
            }
        }

        possibleMove.Targets = result;
    }

    public UnitVisibility DetermineLineOfSight((int X, int Y) attackPosition, IChessPiece attacker, IChessPiece defender)
    {
        // Send 3 rays - depending on the direction of attack, to the side and behind or into the target
        // this allows to determine visibility if the target is at least slightly behind cover, but still should be visible, logically
        // could prevent 'feels bad' moments? needs testing

        var directionOfAttack = DirectionConverter.CalculateClosestDirection(new Vector2(attackPosition.X, attackPosition.Y), defender.GetPositionVector2());
        //directionOfAttack = DirectionConverter.GetOppositeDirection(directionOfAttack);
        var vectorOfAttack = DirectionConverter.ConvertToVector2(directionOfAttack);
        var positionsToCheck = new List<(int X, int Y)>()
        {
            (defender.XBoard, defender.YBoard + vectorOfAttack.Y),
            (defender.XBoard + vectorOfAttack.X, defender.YBoard),
        };
        if (vectorOfAttack.X != 0 && vectorOfAttack.Y != 0) // Add the normal position of the piece if the direction is a skew, just in case the piece is in a corner
        {
            positionsToCheck.Add((defender.XBoard, defender.YBoard));
        }
        Debug.Log($"Closest direction: {directionOfAttack}\n" +
            $"Checking visibility for tiles: {positionsToCheck.First().X}, {positionsToCheck.First().Y} and {positionsToCheck.Last().X}, {positionsToCheck.Last().Y}");

        var visibilityChecks = new List<bool>();
        foreach (var position in positionsToCheck)
        {
            if (IsPositionOnBoard(position.X, position.Y))
            {
                var result = IsVisible(attackPosition, position, attacker, defender);
                visibilityChecks.Add(result);
            }
        }

        var visibilityResult = UnitVisibility.NotVisible;

        if (visibilityChecks.Count == 1)
        {
            if (visibilityChecks.First())
            {
                visibilityResult = UnitVisibility.FullyVisible;
            }
        }
        else
        {
            var successes = 0;
            foreach (var check in visibilityChecks)
            {
                if (check)
                {
                    successes++;
                }
            }

            switch (successes)
            {
                case 1:
                    visibilityResult = UnitVisibility.InCover;
                    break;
                case 2 or 3:
                    visibilityResult = UnitVisibility.FullyVisible;
                    break;
                default:
                    break;
            }
        }

        return visibilityResult;
    }

    private bool IsVisible((int X, int Y) start, (int X, int Y) end, IChessPiece attacker, IChessPiece target)
    {
        // Bresenham's line drawing algorithm
        // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        // Check each space on a line between 'start' and 'end'

        var path = new List<(int X, int Y)>();
        var x1 = start.X; var y1 = start.Y;
        var x2 = end.X; var y2 = end.Y;
        int w = x2 - x1;
        int h = y2 - y1;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i < longest; i++)
        {
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x1 += dx1;
                y1 += dy1;
            }
            else
            {
                x1 += dx2;
                y1 += dy2;
            }

            if (IsPositionOnBoard(x1, y1))
            {
                var boardPosition = GetPosition(x1, y1);
                // If there is anything in the way, stop calculating the ray
                if (boardPosition != null && boardPosition != target && boardPosition != attacker)
                {
                    return false;
                }
            }
            else
            {
                Debug.Log($"Not on board: {x1}, {y1}. Start {start.X}, {start.Y}, End {end.X}, {end.Y}");

                return false;
            }
        }

        return true;
    }


    public IChessPiece PerformAttack(PossibleMove move)
    {
        if (move.RemovedChessPiece == null)
        {
            return null;
        }

        var attackDiceToThrow = move.ChessPiece.Firepower;
        var defenseDiceToThrow = move.RemovedChessPiece.Survivability;

        // Calculate if attacker is striking from a flank
        if (DirectionConverter.IsAttackingSide(move.ChessPiece, move.RemovedChessPiece)
            && defenseDiceToThrow > 0)
        {
            defenseDiceToThrow--;
        }

        var attackThrowResults = DiceRoller.RollCombatDice(attackDiceToThrow);
        var defenseThrowResults = DiceRoller.RollCombatDice(defenseDiceToThrow);

        int damageDealt = 0;
        int defenseDiceIndex = 0;

        foreach (var attack in attackThrowResults)
        {
            // If dice results are the same, or defense is higher than attack, discards both dice
            if (attack <= defenseThrowResults[defenseDiceIndex])
            {
                defenseDiceIndex++;
                continue;
            }
            else
            {
                // else, add the attack damage to damage dealt
                damageDealt += attack;
            }
        }

        move.RemovedChessPiece.Health -= damageDealt;

        return move.RemovedChessPiece;
    }
}