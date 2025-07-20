using Models;
using UnityEngine;

namespace Components
{
    public class MovePlate : MonoBehaviour
    {
        public GameObject controller = null;

        // private GameObject _pieceReference = null!;
        // public GameObject PieceReference { get => _pieceReference; set { _pieceReference = value; } }

        // Board positions, not world coordinates
        // private int _matrixX;
        // public int MatrixX { get => _matrixX; set { _matrixX = value; } }
        // private int _matrixY;
        // public int MatrixY { get => _matrixY; set { _matrixY = value; } }


        public PossibleMove Move { get; set; } = null;

        public float Depth { get; set; } = 1;

        public void Start()
        {
            if (Move != null && Move.RemovedChessPiece != null)
            {
                // Change to red color
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0, 0);
                return;
            }
        }
    }
}