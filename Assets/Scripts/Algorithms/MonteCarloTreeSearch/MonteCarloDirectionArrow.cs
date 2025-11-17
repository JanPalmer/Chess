using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonteCarloDirectionArrow : DirectionArrow
{
    public static double ExplorationParameter = 1.41d;

    public string Id = "";

    public MonteCarloDirectionArrow Parent = null;
    public List<MonteCarloDirectionArrow> Children = null;
    public int NumberOfVisits = 0;
    //public int NumberOfWins = 0;
    public int Value = 0;
    public bool IsTerminal = false;

    public IChessPiece Target = null;

    public MonteCarloDirectionArrow(DirectionArrow arrow) : base(arrow) { }
    public double GetUCT()
    {
        if (NumberOfVisits == 0)
        {
            return double.MaxValue;
        }

        return (double)Value / NumberOfVisits + ExplorationParameter * Math.Sqrt(Parent.NumberOfVisits) / NumberOfVisits;
    }
}
