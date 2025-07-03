using System;
using Components;
using Enums;
using Models;
using UnityEngine;

public class DirectionArrowComponent : MonoBehaviour
{
    public DirectionArrow ArrowInfo { get; set; }

    public void Initialize(DirectionArrow arrowInfo)
    {
        ArrowInfo = arrowInfo;

        var vector = DirectionConverter.Convert(ArrowInfo.Direction);

        // Starting Vector2 is (1, 0), because base Arrow sprite is turned right by default
        float angle = Vector2.SignedAngle(new Vector2(1, 0), new Vector2(vector.X, vector.Y));
        this.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Start()
    {
        var brightness = (float)Math.Cos(1 / ArrowInfo.Depth);
        this.GetComponent<SpriteRenderer>().color = new Color(brightness, brightness, brightness);
        Debug.Log($"Arrow brightness = {brightness}");
    }

    public void OnMouseUp()
    {
        Debug.Log($"Arrow OnMouseUp - {ArrowInfo.Move.End.X}, {ArrowInfo.Move.End.Y}, {ArrowInfo.Direction}");

        if (ArrowInfo.Move == null)
        {
            return;
        }

        var game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        var pieceObj = game.GetChesspiece(ArrowInfo.Move.Start.X, ArrowInfo.Move.Start.Y).GetComponent<ChessmanComponent>();
        pieceObj.DestroyMovePlates();
        pieceObj.MoveChessPiece(ArrowInfo.Move, ArrowInfo.Direction);

        game.NextTurn().ConfigureAwait(false);
    }
}
