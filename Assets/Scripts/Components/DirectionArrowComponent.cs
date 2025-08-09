using System;
using Components;
using Infrastructure;
using UnityEngine;

public class DirectionArrowComponent : MonoBehaviour
{
    public DirectionArrow ArrowInfo { get; set; }

    public void Initialize(DirectionArrow arrowInfo)
    {
        ArrowInfo = arrowInfo;

        var vector = DirectionConverter.ConvertToVector2(ArrowInfo.Direction);

        // Starting Vector2 is (1, 0), because base Arrow sprite is turned right by default
        this.transform.rotation = TransformCalculator.CalculateRotation(new Vector2(1, 0), new Vector2(vector.X, vector.Y));
    }

    public void Start()
    {
        var brightness = (float)Math.Sin(1.0f / ArrowInfo.Depth) + 0.1f;
        this.GetComponent<SpriteRenderer>().color = new Color(brightness, brightness, brightness);
        //Debug.Log($"Arrow brightness = {brightness}, depth = {ArrowInfo.Depth}");
    }

    public void OnMouseUp()
    {
        Debug.Log($"Arrow OnMouseUp - {ArrowInfo.Move.End.X}, {ArrowInfo.Move.End.Y}, {ArrowInfo.Direction}");

        if (ArrowInfo.Move == null)
        {
            return;
        }

        var game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        // var pieceObj = game.GetChesspiece(ArrowInfo.Move.Start.X, ArrowInfo.Move.Start.Y).GetComponent<ChessmanComponent>();

        // if (game.HighlightedMove == ArrowInfo.Move)
        // {
        //     game.DestroyMovePlates();
        //     game.NextTurn().ConfigureAwait(false);
        // }
        // // else
        // // {
        // //     game.HighlightedMove = ArrowInfo.Move;
        // // }

        // pieceObj.Move(ArrowInfo.Move, ArrowInfo.Direction);

        game.HighlightedArrow = ArrowInfo;
    }
}
