using Models;
using UnityEngine;

namespace Components
{
    public class MovePlate : MonoBehaviour
    {
        public GameObject controller = null!;

        // private GameObject _pieceReference = null!;
        // public GameObject PieceReference { get => _pieceReference; set { _pieceReference = value; } }

        // Board positions, not world coordinates
        // private int _matrixX;
        // public int MatrixX { get => _matrixX; set { _matrixX = value; } }
        // private int _matrixY;
        // public int MatrixY { get => _matrixY; set { _matrixY = value; } }


        // false: movement, true: attacking another piece
        private bool _attack = false;
        public bool Attack { get => _attack; set { _attack = value; } }

        private PossibleMove _move = null!;
        public PossibleMove Move { get => _move; set { _move = value; } }

        public void Start()
        {
            if (Attack)
            {
                // Change to red color
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0, 0);
            }
        }

        public void OnMouseUp()
        {
            var game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

            var pieceObj = game.GetChesspiece(Move.Start.X, Move.Start.Y).GetComponent<ChessmanObject>();
            pieceObj.DestroyMovePlates();
            pieceObj.MoveChessPiece(Move);

            game.NextTurn().ConfigureAwait(false);
        }

        // public void SetCoords(int x, int y)
        // {
        //     MatrixX = x;
        //     MatrixY = y;
        // }
    }
}