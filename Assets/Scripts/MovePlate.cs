using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    private GameObject _pieceReference = null;
    public GameObject PieceReference { get => _pieceReference; set { _pieceReference = value; } }

    // Board positions, not world coordinates
    private int _matrixX;
    public int MatrixX { get => _matrixX; set { _matrixX = value; } }
    private int _matrixY;
    public int MatrixY { get => _matrixY; set { _matrixY = value; } }


    // false: movement, true: attacking another piece
    public bool attack = false;

    public void Start()
    {
        if (attack)
        {
            // Change to red color
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0, 0);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        if (attack)
        {
            // GetPosition return the chesspiece GameObject that the movePlate is set on
            GameObject chessPiece = controller.GetComponent<Game>().GetPosition(_matrixX, _matrixY);

            if (chessPiece.name.Contains("king"))
            {
                var winningPiece = _pieceReference.GetComponent<Chessman>();
                controller.GetComponent<Game>().Winner(winningPiece.Player);
            }

            Destroy(chessPiece);
        }

        var currentPiece = _pieceReference.GetComponent<Chessman>();
        // Empty up the position from which the piece is moving
        (var pastX, var pastY) = (currentPiece.XBoard, currentPiece.YBoard);
        controller.GetComponent<Game>().SetPositionEmpty(pastX, pastY);

        // Move piece to the chosen position
        currentPiece.XBoard = _matrixX;
        currentPiece.YBoard = _matrixY;
        currentPiece.SetCoords();

        var game = controller.GetComponent<Game>();
        game.SetPosition(_pieceReference);

        //game.NextTurn();

        currentPiece.DestroyMovePlates();

        game.NextTurn();
    }

    public void SetCoords(int x, int y)
    {
        MatrixX = x;
        MatrixY = y;
    }
}
