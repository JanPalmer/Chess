using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEngine;

namespace Components
{
    public class ChessmanObject : MonoBehaviour
    {
        // References
        public GameObject Controller { get => GameObject.FindGameObjectWithTag("GameController"); }
        public GameObject MovePlate;
        public GameObject MovePlateOrange;
        public GameObject MovePlateBlue;

        public Chessman ChessPieceInformation = null!;

        public SpriteLibrary SpriteLibrary;
        private GameObject _movePlateColored;

        public Chessman ToChessman()
        {
            return new Chessman(ChessPieceInformation);
        }

        public void Activate(string name, int x, int y, Board board)
        {
            SpriteLibrary = Controller.GetComponent<SpriteLibrary>();

            // take the instantianted location and adjust the transform
            this.name = name;
            ChessPieceInformation = new Chessman()
            {
                XBoard = x,
                YBoard = y,
                Player = InitializePlayer(),
                Role = InitializeChessPieceRole(),
                Board = board,
            };

            Debug.Log(this.name);

            // set sprite
            this.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.SelectSprite(this.name);

            SetCoords();

            // if (ChessPieceInformation.Player == PlayerSide.White)
            // {
            //     // GameObject mp = Instantiate(MovePlateOrange, new Vector3(x, y, -3.0f), Quaternion.identity);
            //     // MovePlateOrange mpScript = mp.GetComponent<MovePlateOrange>();

            //     //GameObject mp = Instantiate(MovePlateOrange, new Vector3(x, y, -3.0f), Quaternion.identity);

            //     //SetDirection(ChessPieceDirection.Up);
            // }
            // else
            // {
            //     //SetDirection(ChessPieceDirection.Down);
            // }

            _movePlateColored = MovePlateSpawn(x, y, ChessPieceInformation.Player);
            _movePlateColored.transform.SetParent(this.transform, true);

            if (ChessPieceInformation.Player == PlayerSide.White)
            {
                SetDirection(ChessPieceDirection.Up);
            }
            else
            {
                SetDirection(ChessPieceDirection.Down);
            }
        }

        public void SetCoords()
        {
            var newCoordinates = CalculateTransform(ChessPieceInformation.XBoard, ChessPieceInformation.YBoard);

            this.transform.position = new Vector3(newCoordinates.X, newCoordinates.Y, -1.0f);
        }

        private void OnMouseUp()
        {
            var game = Controller!.GetComponent<Game>();

            if (!game.IsGameOver && game.CurrentPlayer == ChessPieceInformation.Player)
            {
                DestroyMovePlates();

                InitiateMovePlates();

                Debug.Log(this.name + " - MovePlates created");
            }
        }

        public void MoveChessPiece(PossibleMove move)
        {
            if (move.RemovedChessPiece != null)
            {
                var game = Controller.GetComponent<Game>();

                var pieceObj = game.GetChesspiece(move.End.X, move.End.Y);
                var chessPiece = pieceObj.GetComponent<ChessmanObject>().ChessPieceInformation;

                if (chessPiece.Role == ChessPieceRole.King)
                {
                    game.Winner(ChessPieceInformation.Player);
                }

                game.RemoveChesspiece(pieceObj);
            }

            Debug.Log($"ChessmanComponent - Moving Chesspiece {ChessPieceInformation.Player} {ChessPieceInformation.Role}");

            ChessPieceInformation.Board.MoveChessPiece(move);

            SetCoords();
        }

        private (float X, float Y) CalculateTransform(int boardX, int boardY)
        {
            float x = boardX;
            float y = boardY;

            x *= 0.66f;
            y *= 0.66f;
            x += -2.3f;
            y += -2.3f;

            return (x, y);
        }

        #region Rotation

        public void SetDirection(ChessPieceDirection direction)
        {
            var vector = DirectionConverter.Convert(direction);

            SetDirection(ChessPieceInformation.XBoard + vector.X, ChessPieceInformation.YBoard + vector.Y);
        }

        /// <summary>
        /// Sets direction of the piece by inputting the coordinates on the board that it wants to look at
        /// </summary>
        /// <param name="x">X coordinate on the board</param>
        /// <param name="y">Y coordinate on the board</param>
        public void SetDirection(int x, int y)
        {
            var directionVector = new Vector2(x - ChessPieceInformation.XBoard, y - ChessPieceInformation.YBoard);
            directionVector.Normalize();

            float angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;

            this.transform.rotation = Quaternion.Euler(0, 0, angle);

            _movePlateColored.transform.rotation = Quaternion.identity;
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
            var moves = ChessPieceInformation.GetPossibleMoves();

            foreach (var move in moves)
            {
                MovePlateSpawn(move);
            }
        }

        public MovePlate MovePlateSpawn(PossibleMove possibleMove)
        {
            var newCoordinates = CalculateTransform(possibleMove.End.X, possibleMove.End.Y);

            GameObject mp = Instantiate(MovePlate, new Vector3(newCoordinates.X, newCoordinates.Y, -3.0f), Quaternion.identity);

            MovePlate mpScript = mp.GetComponent<MovePlate>();

            mpScript.Move = possibleMove;

            return mpScript;
        }

        public GameObject MovePlateSpawn(int x, int y, PlayerSide player)
        {
            var newCoordinates = CalculateTransform(x, y);
            GameObject mp = null;

            if (player == PlayerSide.White)
            {
                mp = Instantiate(MovePlateOrange, new Vector3(newCoordinates.X, newCoordinates.Y, -0.5f), Quaternion.identity);
            }
            else
            {
                mp = Instantiate(MovePlateBlue, new Vector3(newCoordinates.X, newCoordinates.Y, -0.5f), Quaternion.identity);
            }

            return mp;
        }

        #endregion

        #region Initialize

        private PlayerSide InitializePlayer()
        {
            if (this.name.Contains("white"))
            {
                return PlayerSide.White;
            }
            else
            {
                return PlayerSide.Black;
            }
        }

        private ChessPieceRole InitializeChessPieceRole()
        {
            return this.name switch
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
}

