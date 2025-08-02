using System.Collections;
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

        public IChessPiece Target { get; set; } = null;
        public UnitVisibility TargetVisibility { get; set; } = UnitVisibility.NotVisible;

        public void Start()
        {

            // else
            // {
            //     gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            // }


            return;
        }

        public void Activate(PossibleMove move, IChessPiece target = null, UnitVisibility targetVisibility = UnitVisibility.NotVisible)
        {
            Move = move;
            Target = target;
            TargetVisibility = targetVisibility;

            if (Target != null)
            {
                // Change to red color, based on visibility
                gameObject.GetComponent<SpriteRenderer>().color = TargetVisibility switch
                {
                    UnitVisibility.FullyVisible => new Color(1.0f, 0, 0), // vibrant red                    
                    UnitVisibility.InCover => new Color(0.6f, 0, 0), // darker shade of red
                    _ => new Color(0, 0, 0),
                };
            }

            //Debug.Log($"MovePlate-Activate(): Target {Target.XBoard}, {Target.YBoard}, Visibility: {TargetVisibility}");
        }
    }
}