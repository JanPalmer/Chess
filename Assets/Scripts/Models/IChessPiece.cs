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
    /// Name
    /// </summary>
    public string Name { get; set; }

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
    public UnitRole Role { get; set; }

    /// <summary>
    /// Marks the piece as removed
    /// </summary>
    public bool IsRemoved { get; }


    // Tank stats
    public int Health { get; set; }

    public int Movement { get; set; }

    public int Firepower { get; set; }

    public int Survivability { get; set; }

    public UnitDirection Direction { get; set; }


    public List<PossibleMove> GetPossibleMoves();

    public Vector2 GetPositionVector2();
    public (int X, int Y) GetPositionTupleXY();
}
