using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algorithms;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Components
{
    public class Game : MonoBehaviour
    {
        public GameObject chesspiece;

        // Positions and team for each chess piece
        //private GameObject[,] positions = new GameObject[8, 8];
        // private GameObject[] playerBlack = new GameObject[16];
        // private GameObject[] playerWhite = new GameObject[16];

        private List<GameObject> _chessPieces { get; set; }

        private Board _board { get; set; }

        private PlayerSide _currentPlayer = PlayerSide.White;
        public PlayerSide CurrentPlayer { get => _currentPlayer; set => _currentPlayer = value; }

        private bool _gameOver = false;
        public bool IsGameOver => _gameOver;

        private IAlgorithm _opponentAlgorithm = null;

        void Start()
        {
            var controller = GameObject.FindGameObjectWithTag("GameController");
            var spriteLibrary = controller.GetComponent<SpriteLibrary>();
            spriteLibrary.Initialize();

            _board = new Board();

            _chessPieces = new List<GameObject>{
                Create("white_rook", 0, 0), Create("white_knight", 1, 0), Create("white_bishop", 2, 0), Create("white_queen", 3, 0),
                Create("white_king", 4, 0), Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
                Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1), Create("white_pawn", 3, 1),
                Create("white_pawn", 4, 1), Create("white_pawn", 5, 1), Create("white_pawn", 6, 1), Create("white_pawn", 7, 1),

                Create("black_rook", 0, 7), Create("black_knight", 1, 7), Create("black_bishop", 2, 7), Create("black_queen", 3, 7),
                Create("black_king", 4, 7), Create("black_bishop", 5, 7), Create("black_knight", 6, 7), Create("black_rook", 7, 7),
                Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6), Create("black_pawn", 3, 6),
                Create("black_pawn", 4, 6), Create("black_pawn", 5, 6), Create("black_pawn", 6, 6), Create("black_pawn", 7, 6),
            };

            // foreach (var piece in playerWhite)
            // {
            //     Board.SetPosition(piece);
            // }

            // foreach (var piece in playerBlack)
            // {
            //     Board.SetPosition(piece);
            // }

            _opponentAlgorithm = new NegamaxAlgorithm();
        }

        public GameObject Create(string name, int x, int y)
        {
            GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
            ChessmanObject cm = obj.GetComponent<ChessmanObject>();

            Console.WriteLine(name);

            cm.Activate(name, x, y, _board);

            _board.SetPosition(cm.ChessPieceInformation, x, y);

            return obj;
        }

        public async Task NextTurn()
        {
            CurrentPlayer = CurrentPlayer == PlayerSide.White
                ? PlayerSide.Black
                : PlayerSide.White;

            Debug.Log($"Next turn - player: {CurrentPlayer}");

            if (CurrentPlayer == PlayerSide.Black)
            {
                if (_opponentAlgorithm != null)
                {
                    var nextMove = _opponentAlgorithm.CalculateNextMove(CurrentPlayer, _board, 2);
                    //var pieceToMove = Board.GetPosition(nextMove.ChessPiece.XBoard, nextMove.ChessPiece.YBoard);
                    var pieceObj = GetChesspiece(nextMove.Start.X, nextMove.Start.Y).GetComponent<ChessmanObject>();

                    pieceObj.MoveChessPiece(nextMove);

                    await NextTurn();
                }
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

        public GameObject GetChesspiece(int x, int y)
        {
            return _chessPieces.SingleOrDefault((obj) =>
            {
                var pieceInfo = obj.GetComponent<ChessmanObject>().ChessPieceInformation;
                return pieceInfo.XBoard == x && pieceInfo.YBoard == y && pieceInfo.IsRemoved == false;
            });
        }

        public void RemoveChesspiece(GameObject chesspiece)
        {
            var pieceInfo = chesspiece.GetComponent<ChessmanObject>().ChessPieceInformation;
            _board.SetPositionEmpty(pieceInfo.XBoard, pieceInfo.YBoard);
            _chessPieces.Remove(chesspiece);
            Destroy(chesspiece);
        }

        public void Winner(PlayerSide playerWinner)
        {
            _gameOver = true;

            var text = GameObject.FindGameObjectWithTag("TextWinner").GetComponent<TextMeshProUGUI>();
            text.enabled = true;
            text.text = playerWinner.ToString() + " is the winner!";

            var textRestart = GameObject.FindGameObjectWithTag("TextRestart").GetComponent<TextMeshProUGUI>();
            textRestart.enabled = true;
        }

        // public void Loser(string playerLoser)
        // {
        //     _gameOver = true;
        //     GameObject.FindGameObjectWithTag("TextWinner").GetComponent<TextMeshPro>().enabled = true;
        // }
    }
}