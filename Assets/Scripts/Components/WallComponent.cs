using Enums;
using Models;
using Unity.VisualScripting;
using UnityEngine;

namespace Components
{
    public class WallComponent : ChessmanComponent
    {
        public override void Activate(string name, int x, int y, Board board)
        {
            this.name = name;
            PieceInfo = new Wall()
            {
                XBoard = x,
                YBoard = y,
                Board = board,
                Role = ChessPieceRole.Wall,
            };

            Debug.Log(this.name);

            SetCoords();
        }

        public override void SetCoords()
        {
            var newCoordinates = TransformCalculator.CalculateTransform(PieceInfo.XBoard, PieceInfo.YBoard);

            this.transform.position = new Vector3(newCoordinates.X, newCoordinates.Y, -1.0f);
        }

        protected override void OnMouseUp()
        {
            return;
        }

        public override void SetDirection(int x, int y)
        {
            return;
        }
    }
}