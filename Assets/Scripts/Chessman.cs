using UnityEngine;

public class Chessman : MonoBehaviour
{
    // References
    public GameObject controller;
    public GameObject movePlate;

    // Positions
    private int _xBoard = -1;
    public int XBoard { get => _xBoard; set { _xBoard = value; } }
    private int _yBoard = -1;
    public int YBoard { get => _yBoard; set { _yBoard = value; } }

    // Variable to keep track if player is "black" or "white"
    private PlayerSide _player;
    public PlayerSide Player { get => _player; set { _player = value; } }

    private ChessPieceRole _role;

    public SpriteLibrary spriteLibrary;

    public void Activate()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        spriteLibrary = controller.GetComponent<SpriteLibrary>();

        // take the instantianted location and adjust the transform
        SetCoords();

        Debug.Log(this.name);
        // set sprite
        this.GetComponent<SpriteRenderer>().sprite = spriteLibrary.SelectSprite(this.name);

        // set player
        InitializePlayer();

        InitializeChessPieceRole();
    }

    public void SetCoords()
    {
        float x = _xBoard;
        float y = _yBoard;

        x *= 0.66f;
        y *= 0.66f;
        x += -2.3f;
        y += -2.3f;

        this.transform.position = new Vector3(x, y, -1.0f);
    }

    private void OnMouseUp()
    {
        var game = controller.GetComponent<Game>();

        if (!game.IsGameOver && game.CurrentPlayer == Player)
        {
            DestroyMovePlates();

            InitiateMovePlates();

            Debug.Log(this.name + " - MovePlates created");
        }
    }

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");

        foreach (var movePlate in movePlates)
        {
            Destroy(movePlate);
        }
    }

    public void InitiateMovePlates()
    {
        switch (_role)
        {
            case ChessPieceRole.Queen:
                LineMovePlate(1, 0);
                LineMovePlate(1, 1);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(-1, -1);
                LineMovePlate(0, -1);
                LineMovePlate(1, -1);
                break;
            case ChessPieceRole.Knight:
                LMovePlate();
                break;
            case ChessPieceRole.Bishop:
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case ChessPieceRole.King:
                SurroundMovePlate();
                break;
            case ChessPieceRole.Rook:
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;
            case ChessPieceRole.Pawn:
                if (Player == PlayerSide.Black)
                {
                    PawnMovePlate(XBoard, YBoard - 1);
                }
                else
                {
                    PawnMovePlate(XBoard, YBoard + 1);
                }
                break;
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();
        int x = XBoard + xIncrement;
        int y = YBoard + yIncrement;

        while (sc.IsPositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.IsPositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().Player != Player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(XBoard + 1, YBoard + 2);
        PointMovePlate(XBoard - 1, YBoard + 2);
        PointMovePlate(XBoard + 2, YBoard + 1);
        PointMovePlate(XBoard - 2, YBoard + 1);
        PointMovePlate(XBoard + 1, YBoard - 2);
        PointMovePlate(XBoard - 1, YBoard - 2);
        PointMovePlate(XBoard + 2, YBoard - 1);
        PointMovePlate(XBoard - 2, YBoard - 1);
    }

    public void SurroundMovePlate()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                PointMovePlate(XBoard + i, YBoard + j);
            }
        }
    }

    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.IsPositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<Chessman>().Player != Player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.IsPositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }

            if (sc.IsPositionOnBoard(x + 1, y)
            && sc.GetPosition(x + 1, y) != null
            && sc.GetPosition(x + 1, y).GetComponent<Chessman>().Player != Player)
            {
                MovePlateAttackSpawn(x + 1, y);
            }

            if (sc.IsPositionOnBoard(x - 1, y)
            && sc.GetPosition(x - 1, y) != null
            && sc.GetPosition(x - 1, y).GetComponent<Chessman>().Player != Player)
            {
                MovePlateAttackSpawn(x - 1, y);
            }
        }
    }

    public MovePlate MovePlateSpawn(int matrixX, int matrixY)
    {
        float x = matrixX;
        float y = matrixY;

        x *= 0.66f;
        y *= 0.66f;

        x += -2.3f;
        y += -2.3f;

        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();

        mpScript.PieceReference = gameObject;
        mpScript.SetCoords(matrixX, matrixY);

        return mpScript;
    }

    public MovePlate MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        var mpScript = MovePlateSpawn(matrixX, matrixY);
        mpScript.Attack = true;

        return mpScript;
    }

    private void InitializePlayer()
    {
        if (this.name.Contains("white"))
        {
            Player = PlayerSide.White;
        }
        else
        {
            Player = PlayerSide.Black;
        }
    }

    private void InitializeChessPieceRole()
    {
        _role = this.name switch
        {
            string a when a.Contains("pawn") => ChessPieceRole.Pawn,
            string a when a.Contains("bishop") => ChessPieceRole.Bishop,
            string a when a.Contains("knight") => ChessPieceRole.Knight,
            string a when a.Contains("rook") => ChessPieceRole.Rook,
            string a when a.Contains("queen") => ChessPieceRole.Queen,
            string a when a.Contains("king") => ChessPieceRole.King,
            _ => ChessPieceRole.Pawn,
        };
    }
}
