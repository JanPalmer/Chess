using System.Collections.Generic;
using System.Linq;
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

        public Chessman ChessPieceInformation = null!;

        public SpriteLibrary SpriteLibrary;

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

            SetCoords();
            Debug.Log(this.name);

            // set sprite
            this.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.SelectSprite(this.name);
        }

        public void SetCoords()
        {
            float x = ChessPieceInformation.XBoard;
            float y = ChessPieceInformation.YBoard;

            x *= 0.66f;
            y *= 0.66f;
            x += -2.3f;
            y += -2.3f;

            this.transform.position = new Vector3(x, y, -1.0f);
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
            float x = possibleMove.End.X;
            float y = possibleMove.End.Y;

            x *= 0.66f;
            y *= 0.66f;

            x += -2.3f;
            y += -2.3f;

            GameObject mp = Instantiate(MovePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

            MovePlate mpScript = mp.GetComponent<MovePlate>();

            mpScript.Move = possibleMove;

            return mpScript;
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

