using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public GameObject chesspiece;

    // Positions and team for each chess piece
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private string _currentPlayer = "white";
    public string CurrentPlayer { get => _currentPlayer; set => _currentPlayer = value; }

    private bool _gameOver = false;
    public bool IsGameOver => _gameOver;

    void Start()
    {
        var controller = GameObject.FindGameObjectWithTag("GameController");
        var spriteLibrary = controller.GetComponent<SpriteLibrary>();
        spriteLibrary.Initialize();

        playerWhite = new GameObject[]{
            Create("white_rook", 0, 0), Create("white_knight", 1, 0), Create("white_bishop", 2, 0), Create("white_queen", 3, 0),
            Create("white_king", 4, 0), Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1), Create("white_pawn", 3, 1),
            Create("white_pawn", 4, 1), Create("white_pawn", 5, 1), Create("white_pawn", 6, 1), Create("white_pawn", 7, 1),
        };

        playerBlack = new GameObject[]{
            Create("black_rook", 0, 7), Create("black_knight", 1, 7), Create("black_bishop", 2, 7), Create("black_queen", 3, 7),
            Create("black_king", 4, 7), Create("black_bishop", 5, 7), Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6), Create("black_pawn", 3, 6),
            Create("black_pawn", 4, 6), Create("black_pawn", 5, 6), Create("black_pawn", 6, 6), Create("black_pawn", 7, 6),
        };

        foreach (var piece in playerWhite)
        {
            SetPosition(piece);
        }

        foreach (var piece in playerBlack)
        {
            SetPosition(piece);
        }

        //Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
    }

    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();

        Console.WriteLine(name);

        cm.name = name;
        cm.XBoard = x;
        cm.YBoard = y;
        cm.Activate();

        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();
        positions[cm.XBoard, cm.YBoard] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool IsPositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1))
        {
            return false;
        }

        return true;
    }

    public void NextTurn()
    {
        if (CurrentPlayer == "white")
        {
            CurrentPlayer = "black";
        }
        else
        {
            CurrentPlayer = "white";
        }
    }

    public void Update()
    {
        if (IsGameOver && Input.GetMouseButtonDown(0))
        {
            _gameOver = false;

            SceneManager.LoadScene("Game");
        }
    }

    public void Winner(string playerWinner)
    {
        _gameOver = true;

        var text = GameObject.FindGameObjectWithTag("TextWinner").GetComponent<TextMeshProUGUI>();
        text.enabled = true;
        text.text = playerWinner + " is the winner!";

        var textRestart = GameObject.FindGameObjectWithTag("TextRestart").GetComponent<TextMeshProUGUI>();
        textRestart.enabled = true;
    }

    // public void Loser(string playerLoser)
    // {
    //     _gameOver = true;
    //     GameObject.FindGameObjectWithTag("TextWinner").GetComponent<TextMeshPro>().enabled = true;
    // }
}
