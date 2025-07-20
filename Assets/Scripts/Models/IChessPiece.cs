using System.Collections.Generic;
using Enums;
using Models;
using UnityEngine;

public interface IChessPiece
{
    // The 0.5 is for turning/movement on the skewed axes, since for 1.0 the distance to the left and right is not registered,
    // while putting 2.0 gives too much movement forward
    public const float MovementDistance = 1.5f;

    /// <summary>
    /// Parent Board to which the piece belongs to
    /// </summary>
    public Board Board { get; set; }

    /// <summary>
    /// Position on the board - X coordinate
    /// </summary>
    public int XBoard { get; set; }
    /// <summary>
    /// Position on the board - Y coordinate
    /// </summary>
    public int YBoard { get; set; }

    /// <summary>
    /// Variable to keep track if player is "black" or "white"
    /// </summary>
    public PlayerSide Player { get; set; }

    /// <summary>
    /// Piece role, like 'Pawn', 'Rook' etc
    /// </summary>
    public ChessPieceRole Role { get; set; }

    /// <summary>
    /// Marks the piece as removed
    /// </summary>
    public bool IsRemoved { get; set; }


    // Tank stats
    public uint Health { get; set; }

    public uint Movement { get; set; }

    public uint Firepower { get; set; }

    public uint Survivability { get; set; }

    public UnitDirection Direction { get; set; }


    public List<PossibleMove> GetPossibleMoves();
}
