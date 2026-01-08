using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algorithms;
using Enums;
using Infrastructure;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Components
{
    public class Game : MonoBehaviour
    {
        public GameObject chesspiece;
        public GameObject wall;

        public GameObject MovePlate;
        public GameObject MovePlateOrange;
        public GameObject MovePlateBlue;
        public GameObject DirectionArrow;

        private const int TreeSearchDepth = 3;

        // Positions and team for each chess piece
        //private GameObject[,] positions = new GameObject[8, 8];
        // private GameObject[] playerBlack = new GameObject[16];
        // private GameObject[] playerWhite = new GameObject[16];

        private List<GameObject> _chessPieces { get; set; }

        private Board _board { get; set; }

        private PlayerSide _currentPlayer = PlayerSide.Orange;
        public PlayerSide CurrentPlayer { get => _currentPlayer; set => _currentPlayer = value; }


        private ChessmanComponent _highlightedPiece = null;
        public ChessmanComponent HighlightedPiece
        {
            get => _highlightedPiece;
            set
            {
                SelectChesspiece(value);
                _highlightedPiece = value;
            }
        }

        private DirectionArrow _highlightedArrow = null;
        public DirectionArrow HighlightedArrow
        {
            get => _highlightedArrow;
            set
            {
                SelectArrow(value);
                _highlightedArrow = value;
            }
        }

        private bool _gameOver = false;
        public bool IsGameOver => _gameOver;

        private IAlgorithm _opponentAlgorithm = null;

        private int _wallCounter = 0;

        void Start()
        {
            var controller = GameObject.FindGameObjectWithTag("GameController");
            var spriteLibrary = controller.GetComponent<SpriteLibrary>();
            spriteLibrary.Initialize();

            _board = new Board();

            // normal chess
            // _chessPieces = new List<GameObject>{
            //     Create("white_rook", 0, 0), Create("white_knight", 1, 0), Create("white_bishop", 2, 0), Create("white_queen", 3, 0),
            //     Create("white_king", 4, 0), Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            //     Create("white_pawn", 0, 1), Create("white_pawn", 1, 1), Create("white_pawn", 2, 1), Create("white_pawn", 3, 1),
            //     Create("white_pawn", 4, 1), Create("white_pawn", 5, 1), Create("white_pawn", 6, 1), Create("white_pawn", 7, 1),

            //     Create("black_rook", 0, 7), Create("black_knight", 1, 7), Create("black_bishop", 2, 7), Create("black_queen", 3, 7),
            //     Create("black_king", 4, 7), Create("black_bishop", 5, 7), Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            //     Create("black_pawn", 0, 6), Create("black_pawn", 1, 6), Create("black_pawn", 2, 6), Create("black_pawn", 3, 6),
            //     Create("black_pawn", 4, 6), Create("black_pawn", 5, 6), Create("black_pawn", 6, 6), Create("black_pawn", 7, 6),
            // };

            _chessPieces = new List<GameObject>{

                // Mapa 1
                CreateChesspiece("white_rook", PlayerSide.Orange, 1, 6),
                CreateChesspiece("white_pawn", PlayerSide.Orange, 1, 4),
                CreateChesspiece("white_pawn", PlayerSide.Orange, 1, 2),

                CreateChesspiece("white_pawn", PlayerSide.Blue, 14, 6),
                CreateChesspiece("white_pawn", PlayerSide.Blue, 14, 4),
                CreateChesspiece("white_rook", PlayerSide.Blue, 14, 2),

                CreateWall(4, 2), CreateWall(4, 1), CreateWall(5, 1), // lower left corner
                CreateWall(3, 8), CreateWall(3, 7), CreateWall(4, 8), // upper left corner
                CreateWall(10, 8), CreateWall(11, 8), CreateWall(11, 7), // upper right corner
                CreateWall(11, 1), CreateWall(12, 1), CreateWall(12, 2), // lower right corner

                CreateWall(7, 7), 
                CreateWall(6, 6), CreateWall(7, 6),
                CreateWall(5, 5), CreateWall(6, 5), CreateWall(5, 4),

                CreateWall(10,5), 
                CreateWall(9, 4), CreateWall(10, 4), CreateWall(8, 3),
                CreateWall(9, 3), //CreateWall(7, 2), 
                CreateWall(8, 2),


                // Mapa 2
                // CreateChesspiece("white_rook", PlayerSide.Orange, 1, 8),
                // CreateChesspiece("white_pawn", PlayerSide.Orange, 1, 6),
                // CreateChesspiece("white_pawn", PlayerSide.Orange, 1, 4),
                // CreateChesspiece("white_rook", PlayerSide.Orange, 1, 1),

                // CreateChesspiece("white_rook", PlayerSide.Blue, 14, 8),
                // CreateChesspiece("white_pawn", PlayerSide.Blue, 14, 6),
                // CreateChesspiece("white_pawn", PlayerSide.Blue, 14, 4),
                // CreateChesspiece("white_rook", PlayerSide.Blue, 14, 1),

                // CreateWall(4, 1), CreateWall(3, 0), CreateWall(4, 0), // lower left corner
                // CreateWall(3, 8), CreateWall(3, 9), CreateWall(4, 9), // upper left corner
                // CreateWall(11, 9), CreateWall(12, 9), CreateWall(12, 8), // upper right corner
                // CreateWall(11, 1), CreateWall(11, 0), CreateWall(12, 0), // lower right corner
                
                // CreateWall(4, 4), CreateWall(4, 5), // left squiggle
                // CreateWall(5, 5), CreateWall(5, 6), 
                // CreateWall(6, 6), CreateWall(6, 7),

                // CreateWall(11, 4), CreateWall(11, 5), // right squiggle
                // CreateWall(10, 5), CreateWall(10, 6),
                // CreateWall(9, 6), CreateWall(9, 7),

                // CreateWall(7, 2), CreateWall(7, 3), // center square
                // CreateWall(8, 2), CreateWall(8, 3), 

                // CreateChesspiece("white_pawn", PlayerSide.Blue, 8, 5),
                // //CreateChesspiece("white_rook", PlayerSide.Orange, 3, 6),  
                // CreateChesspiece("white_pawn", PlayerSide.Orange, 3, 6),
                // // CreateChesspiece("white_pawn", PlayerSide.Orange, 3, 4),
                // // CreateChesspiece("white_pawn", PlayerSide.Orange, 3, 2),
                // CreateChesspiece("white_rook", PlayerSide.Blue, 6, 4),
                // CreateChesspiece("white_rook", PlayerSide.Blue, 5, 4),
                // CreateChesspiece("white_rook", PlayerSide.Blue, 4, 4),
                // CreateChesspiece("white_pawn", PlayerSide.Orange, 5, 6),

                // CreateChesspiece("white_rook", PlayerSide.Blue, 3, 2),
                // CreateWall(5, 2),
                // CreateChesspiece("white_rook", PlayerSide.Orange, 6, 2),

                // CreateWall(6, 5)
            };

            // foreach (var piece in playerWhite)
            // {
            //     Board.SetPosition(piece);
            // }

            // foreach (var piece in playerBlack)
            // {
            //     Board.SetPosition(piece);
            // }

            //_opponentAlgorithm = new NegamaxBasicAlgorithm();
            //_opponentAlgorithm = new NegamaxAlgorithm();
            //_opponentAlgorithm = new MonteCarloAlgorithm();
            _opponentAlgorithm = new NegaScoutAlgorithm();
        }

        public GameObject CreateChesspiece(string spriteName, PlayerSide player, int x, int y)
        {
            GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
            ChessmanComponent cm = obj.GetComponent<ChessmanComponent>();

            Console.WriteLine(spriteName);

            cm.Activate(spriteName, player, x, y, _board);

            _board.SetPosition(cm.PieceInfo, x, y);

            return obj;
        }

        public GameObject CreateWall(int x, int y)
        {
            GameObject obj = Instantiate(wall, new Vector3(0, 0, -1), Quaternion.identity);
            WallComponent cm = obj.GetComponent<WallComponent>();

            var wallName = "wall" + (++_wallCounter).ToString();

            Console.WriteLine(wallName);

            cm.Activate(wallName, PlayerSide.NPC, x, y, _board);

            _board.SetPosition(cm.PieceInfo, x, y);

            return obj;
        }

        public async Task NextTurn()
        {
            HighlightedArrow = null;
            HighlightedPiece = null;

            UpdateMovePlatesForPieces();

            var pieces = _board.GetAllPieces();

            // Check for win condition
            if (_board.GetPiecesForPlayer(CurrentPlayer.GetOpposingPlayer()).Count == 0)
            {
                Winner(CurrentPlayer);
                return;
            }

            // if a new turn has started and all pieces are ready, always start from the Orange player
            if (_board.AreAllPiecesReady())
            {
                CurrentPlayer = PlayerSide.Orange;
                return;
            }

            // If the opposing player has already moved with all of his pieces, don't swap player to allow for chain movements
            if (_board.GetReadyPiecesForPlayer(CurrentPlayer.GetOpposingPlayer()).Count > 0)
            {
                CurrentPlayer = CurrentPlayer.GetOpposingPlayer();
            }

            Debug.Log($"Next turn - player: {CurrentPlayer}");


            if (CurrentPlayer == PlayerSide.Blue)
            {
                if (_opponentAlgorithm != null)
                {
                    DirectionArrow nextArrow = null;

                    await Task.Run(() =>
                    {
                    });

                    nextArrow = _opponentAlgorithm.CalculateNextMove(CurrentPlayer, _board, TreeSearchDepth);


                    var pieceObj = GetChesspiece(nextArrow.Move.Start.X, nextArrow.Move.Start.Y);
                    var pieceChessman = pieceObj.GetComponent<ChessmanComponent>();

                    pieceChessman.Move(nextArrow.Move, nextArrow.Direction);

                    if (nextArrow.Move.AttackedChessPiece != null)
                    {
                        PerformAttack(nextArrow.Move, nextArrow.Move.AttackedChessPiece);
                    }

                    await Task.Delay(500);

                    await NextTurn().ConfigureAwait(false);
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
            try
            {
                return _chessPieces.SingleOrDefault((obj) =>
                {
                    var component = obj.GetComponent<ChessmanComponent>();
                    var pieceInfo = (Chessman)component.PieceInfo;
                    if (pieceInfo.Player == PlayerSide.NPC)
                    {
                        return false;
                    }
                    //Debug.Log("GetChesspiece: " + component.name);
                    return pieceInfo.XBoard == x && pieceInfo.YBoard == y;
                });
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            return null;
        }

        public void RemoveChesspiece(GameObject chesspiece)
        {
            var pieceInfo = chesspiece.GetComponent<ChessmanComponent>().PieceInfo;
            // _board.SetPositionEmpty(pieceInfo.XBoard, pieceInfo.YBoard);
            // _chessPieces.Remove(chesspiece);
            // Destroy(chesspiece);
        }

        public void Winner(PlayerSide playerWinner)
        {
            _gameOver = true;

            var text = GameObject.FindGameObjectWithTag("TextWinner").GetComponent<TextMeshProUGUI>();
            text.enabled = true;
            text.text = playerWinner.ToString() + " is the winner!";

            var textRestart = GameObject.FindGameObjectWithTag("TextRestart").GetComponent<TextMeshProUGUI>();
            textRestart.enabled = true;

            Debug.Log($"Winner: {playerWinner}");
        }

        // public void Loser(string playerLoser)
        // {
        //     _gameOver = true;
        //     GameObject.FindGameObjectWithTag("TextWinner").GetComponent<TextMeshPro>().enabled = true;
        // }


        #region Object selection - UI

        public void SelectChesspiece(ChessmanComponent piece)
        {
            if (piece != HighlightedPiece)
            {
                if (HighlightedPiece != null && HighlightedArrow != null)
                {
                    HighlightedPiece.GetComponent<ChessmanComponent>().UndoMove(HighlightedArrow.Move);
                }

                _highlightedArrow = null;

                DestroyMovePlates();

                Debug.Log("SelectChesspiece - different piece");

                if (piece != null)
                {
                    InitiateMovePlates(piece.PieceInfo);
                }
            }
        }

        public void SelectArrow(DirectionArrow arrow)
        {
            DestroyMovePlates();

            if (arrow == null)
            {
                return;
            }
            Debug.Log("SelectArrow: " + arrow.Direction + ", pos: " + arrow.Move.Start.X + ", " + arrow.Move.Start.Y);


            if (HighlightedArrow == null || !arrow.Equals(HighlightedArrow))
            {
                Debug.Log("Different arrow - " + arrow.Move.End.X + ", " + arrow.Move.End.Y);
                if (HighlightedArrow != null)
                {
                    Debug.Log("Highlighter arrow - " + HighlightedArrow.Move.End.X + ", " + HighlightedArrow.Move.End.Y);
                    HighlightedPiece.GetComponent<ChessmanComponent>().UndoMove(HighlightedArrow.Move);
                }

                //InitiateMovePlates(arrow.Move.ChessPiece);

                //HighlightedPiece.GetComponent<ChessmanComponent>().UndoMove(HighlightedArrow.Move);
                InitiateMovePlates(arrow.Move.ChessPiece);

                var pieceObj = GetChesspiece(arrow.Move.Start.X, arrow.Move.Start.Y).GetComponent<ChessmanComponent>();
                pieceObj.Move(arrow.Move, arrow.Direction);
                InitiateMovePlatesAttack(arrow.Move);
            }
            else
            {
                Debug.Log("Next turn");
                // PerformShooting()
                NextTurn().ConfigureAwait(false);
            }
        }

        public void PerformAttack(PossibleMove move, IChessPiece target)
        {
            var defender = _board.PerformAttack(move, target);
            var chessObject = GetChesspiece(defender.XBoard, defender.YBoard);
            chessObject.GetComponent<ChessmanComponent>().SetHealth();

            move.AttackedChessPiece = (Chessman)chessObject.GetComponent<ChessmanComponent>().PieceInfo;

            if (defender.IsRemoved)
            {
                RemoveChesspiece(chessObject);
                //_board.SetPositionEmpty(defender.XBoard, defender.YBoard);
            }

            //NextTurn().ConfigureAwait(false);
        }

        #endregion


        #region MovePlate

        public void DestroyMovePlates()
        {
            // Don't destroy those plates 

            GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");

            foreach (var movePlate in movePlates)
            {
                Destroy(movePlate);
            }

            DestroyDirectionArrows();
        }

        public void DestroyAttackMovePlates()
        {
            GameObject[] movePlateObjects = GameObject.FindGameObjectsWithTag("MovePlate");

            foreach (var movePlateObject in movePlateObjects)
            {
                var movePlate = movePlateObject.GetComponent<MovePlate>();
                if(movePlate == null || movePlate.Target == null)
                {
                    continue;
                }
                Destroy(movePlate);
            }

            //DestroyDirectionArrows();
        }

        public void InitiateMovePlates(IChessPiece piece)
        {
            var moves = piece.GetPossibleMoves();

            foreach (var move in moves)
            {
                MovePlateSpawn(move);

                //Debug.Log($"Move plate spawned - {move.End.X}, {move.End.Y}");
            }
        }

        public MovePlate MovePlateSpawn(PossibleMove possibleMove)
        {
            GameObject mp = Instantiate(MovePlate, new Vector3(possibleMove.End.X, possibleMove.End.Y, -3.0f), Quaternion.identity);

            var newCoordinates = TransformCalculator.CalculateTransform(possibleMove.End.X, possibleMove.End.Y);
            mp.transform.position = new Vector3(newCoordinates.X, newCoordinates.Y, -3.0f);

            MovePlate mpScript = mp.GetComponent<MovePlate>();

            mpScript.Move = possibleMove;

            foreach (var direction in possibleMove.Directions)
            {
                DirectionArrowSpawn(possibleMove, direction);
            }

            return mpScript;
        }

        public void InitiateMovePlatesAttack(PossibleMove move)
        {
            if (move.Targets == null || move.Targets.Count <= 0)
            {
                return;
            }

            foreach (var target in move.Targets)
            {
                if (target.PossibleTarget == null)
                {
                    continue;
                }

                MovePlateSpawnAttack(move, target.PossibleTarget, target.Visibility);

                //Debug.Log($"Move plate spawned - {move.End.X}, {move.End.Y}");
            }
        }

        public MovePlate MovePlateSpawnAttack(PossibleMove move, IChessPiece target = null, UnitVisibility visibility = UnitVisibility.NotVisible)
        {
            GameObject mp = Instantiate(MovePlate, new Vector3(target.XBoard, target.YBoard, -3.5f), Quaternion.identity);

            var newCoordinates = TransformCalculator.CalculateTransform(target.XBoard, target.YBoard);
            mp.transform.position = new Vector3(newCoordinates.X, newCoordinates.Y, -3.5f);

            MovePlate mpScript = mp.GetComponent<MovePlate>();

            mpScript.Activate(move, target, visibility);

            return mpScript;
        }

        public GameObject MovePlateColoredSpawn(int x, int y, PlayerSide player)
        {
            var newCoordinates = TransformCalculator.CalculateTransform(x, y);
            GameObject mp = null;

            // -0.5f to spawn the plate below the piece, so it doesn't interfere with collision checking on click
            if (player == PlayerSide.Orange)
            {
                mp = Instantiate(MovePlateOrange, new Vector3(newCoordinates.X, newCoordinates.Y, -0.5f), Quaternion.identity);
            }
            else
            {
                mp = Instantiate(MovePlateBlue, new Vector3(newCoordinates.X, newCoordinates.Y, -0.5f), Quaternion.identity);
            }

            return mp;
        }

        public void UpdateMovePlatesForPieces()
        {
            foreach (var piece in _chessPieces)
            {
                var chessmanComp = piece.GetComponent<ChessmanComponent>();
                chessmanComp.UpdateReadyColor();
            }
        }

        public GameObject DirectionArrowSpawn(PossibleMove move, DirectionArrow directionArrow)
        {
            var vector = DirectionConverter.ConvertToVector2(directionArrow.Direction);

            // coordinates are adjusted so the arrows are not directly on the border,
            // possibly interfering with arrows from adjacent squares
            var xCoords = move.End.X + vector.X / 2.8f;
            var yCoords = move.End.Y + vector.Y / 2.8f;

            //Debug.Log($"Spawn Arrow - {xCoords}, {yCoords}");

            var coordinates = TransformCalculator.CalculateTransform(xCoords, yCoords);

            GameObject arrowObj = Instantiate(DirectionArrow, new Vector3(coordinates.X, coordinates.Y, -3.5f), Quaternion.identity);
            arrowObj.GetComponent<DirectionArrowComponent>().Initialize(directionArrow);

            return arrowObj;
        }

        public void DestroyDirectionArrows()
        {
            GameObject[] directionaArrows = GameObject.FindGameObjectsWithTag("DirectionArrow");

            foreach (var arrow in directionaArrows)
            {
                Destroy(arrow);
            }
        }

        #endregion
    }
}