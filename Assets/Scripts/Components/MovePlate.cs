using System.Collections;
using Infrastructure;
using Models;
using UnityEngine;

namespace Components
{
    public class MovePlate : MonoBehaviour
    {
        public GameObject Controller { get => GameObject.FindGameObjectWithTag("GameController"); }

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

        public Color ColorBase;
        public Color ColorDark;

        public void Start()
        {
            ColorBase = gameObject.GetComponent<SpriteRenderer>().color;
            ColorDark = new Color(ColorBase.r * 0.6f, ColorBase.g * 0.6f, ColorBase.b * 0.6f);

            return;
        }

        public void SwapColor(Color color)
        {
            gameObject.GetComponent<SpriteRenderer>().color = color;
        }

        public void Activate(PossibleMove move, IChessPiece target = null, UnitVisibility targetVisibility = UnitVisibility.NotVisible)
        {
            Move = move;
            Target = target;
            TargetVisibility = targetVisibility;

            if (Target != null)
            {
                // Change to red color, based on visibility
                var movePlateColor = TargetVisibility switch
                {
                    UnitVisibility.FullyVisible => new Color(1.0f, 0, 0), // vibrant red                    
                    UnitVisibility.InCover => new Color(0.6f, 0, 0), // darker shade of red
                    _ => new Color(0, 0, 0),
                };

                if(target != null && DirectionConverter.IsAttackingSide(move.ChessPiece, (Chessman)target))
                {
                    Debug.Log("IsAttackingSide - true");
                    movePlateColor.g = 0.45f;
                    movePlateColor.b = 0.45f;
                }

                this.GetComponent<SpriteRenderer>().color = movePlateColor;
            }

            //Debug.Log($"MovePlate-Activate(): Target {Target.XBoard}, {Target.YBoard}, Visibility: {TargetVisibility}");
        }

        public void OnMouseUp()
        {
            if (Target != null)
            {
                Controller.GetComponent<Game>().PerformAttack(Move, Target);
                Controller.GetComponent<Game>().NextTurn().ConfigureAwait(false);
            }
        }
    }
}