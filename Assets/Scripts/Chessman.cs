using System.Collections.Generic;
using System.Linq;
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

    #region Piece movement patterns

    public List<PossibleMove> GetPossibleMoves()
    {
        var moves = new List<PossibleMove>();

        switch (_role)
        {
            case ChessPieceRole.Queen:
                moves = LineMovePattern(1, 0);
                moves.AddRange(LineMovePattern(1, 1));
                moves.AddRange(LineMovePattern(0, 1));
                moves.AddRange(LineMovePattern(-1, 1));
                moves.AddRange(LineMovePattern(-1, 0));
                moves.AddRange(LineMovePattern(-1, -1));
                moves.AddRange(LineMovePattern(0, -1));
                moves.AddRange(LineMovePattern(1, -1));
                break;
            case ChessPieceRole.Knight:
                moves = LMovePattern();
                break;
            case ChessPieceRole.Bishop:
                moves = LineMovePattern(1, 1);
                moves.AddRange(LineMovePattern(1, -1));
                moves.AddRange(LineMovePattern(-1, 1));
                moves.AddRange(LineMovePattern(-1, -1));
                break;
            case ChessPieceRole.King:
                moves = SurroundMovePattern();
                break;
            case ChessPieceRole.Rook:
                moves = LineMovePattern(1, 0);
                moves.AddRange(LineMovePattern(0, 1));
                moves.AddRange(LineMovePattern(-1, 0));
                moves.AddRange(LineMovePattern(0, -1));
                break;
            case ChessPieceRole.Pawn:
                if (Player == PlayerSide.Black)
                {
                    moves = PawnMovePattern(XBoard, YBoard - 1);
                }
                else
                {
                    moves = PawnMovePattern(XBoard, YBoard + 1);
                }
                break;
        }

        return moves;
    }

    public List<PossibleMove> LineMovePattern(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();
        int x = XBoard + xIncrement;
        int y = YBoard + yIncrement;

        var result = new List<PossibleMove>();

        while (sc.IsPositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            result.Add(new PossibleMove(x, y));

            x += xIncrement;
            y += yIncrement;
        }

        if (sc.IsPositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().Player != Player)
        {
            result.Add(new PossibleMove(x, y, isAttack: true));
        }

        return result;
    }

    public List<PossibleMove> LMovePattern()
    {
        var result = new List<PossibleMove>();
        var possibleVectors = new List<(int x, int y)>(){
            (1, 2),
            (-1, 2),
            (2, 1),
            (-2, 1),
            (1, -2),
            (-1, -2),
            (2, -1),
            (-2, -1),
        };

        foreach (var vector in possibleVectors)
        {
            if (PointMovePlate(XBoard + vector.x, YBoard + vector.y, out var possibleMove))
            {
                result.Add(possibleMove);
            }
        }

        return result;
    }

    public List<PossibleMove> SurroundMovePattern()
    {
        var possibleMoves = new List<PossibleMove>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (PointMovePlate(XBoard + i, YBoard + j, out var possibleMove))
                {
                    possibleMoves.Add(possibleMove);
                }
            }
        }

        return possibleMoves;
    }

    public bool PointMovePlate(int x, int y, out PossibleMove possibleMove)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.IsPositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                possibleMove = new PossibleMove(x, y);
                return true;
            }
            else if (cp.GetComponent<Chessman>().Player != Player)
            {
                possibleMove = new PossibleMove(x, y, isAttack: true);
                return true;
            }
        }

        possibleMove = null;
        return false;
    }

    public List<PossibleMove> PawnMovePattern(int x, int y)
    {
        var result = new List<PossibleMove>();

        Game sc = controller.GetComponent<Game>();
        if (sc.IsPositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            // Move forward
            if (cp == null)
            {
                result.Add(new PossibleMove(x, y));
            }

            // Or attack diagonally
            if (sc.IsPositionOnBoard(x + 1, y)
            && sc.GetPosition(x + 1, y) != null
            && sc.GetPosition(x + 1, y).GetComponent<Chessman>().Player != Player)
            {
                result.Add(new PossibleMove(x + 1, y, isAttack: true));
            }

            if (sc.IsPositionOnBoard(x - 1, y)
            && sc.GetPosition(x - 1, y) != null
            && sc.GetPosition(x - 1, y).GetComponent<Chessman>().Player != Player)
            {
                result.Add(new PossibleMove(x - 1, y, isAttack: true));
            }
        }

        return result;
    }

    #endregion

    #region MovePlate

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
        var moves = GetPossibleMoves();

        foreach (var move in moves)
        {
            MovePlateSpawn(move);
        }
    }

    public MovePlate MovePlateSpawn(int matrixX, int matrixY, bool isAttack = false)
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
        mpScript.Attack = isAttack;

        return mpScript;
    }

    public MovePlate MovePlateSpawn(PossibleMove possibleMove)
    {
        return MovePlateSpawn(possibleMove.X, possibleMove.Y, possibleMove.IsAttack);
    }

    #endregion

    #region Initialize

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

    #endregion
}