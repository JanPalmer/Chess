using System.Linq;
using Enums;
using Models;
using UnityEngine;

namespace Components
{
    public class ChessmanComponent : MonoBehaviour
    {
        // References
        public GameObject Controller { get => GameObject.FindGameObjectWithTag("GameController"); }
        public GameObject MovePlate;
        public GameObject MovePlateOrange;
        public GameObject MovePlateBlue;
        public GameObject DirectionArrow;

        public IChessPiece PieceInfo = null!;

        public SpriteLibrary SpriteLibrary;
        private GameObject _movePlateColored;

        public Chessman ToChessman()
        {
            return new Chessman(PieceInfo);
        }

        public virtual void Activate(string name, int x, int y, Board board)
        {
            SpriteLibrary = Controller.GetComponent<SpriteLibrary>();

            // take the instantianted location and adjust the transform
            this.name = name;
            PieceInfo = new Chessman()
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

            _movePlateColored = MovePlateColoredSpawn(x, y, PieceInfo.Player);
            _movePlateColored.transform.SetParent(this.transform, true);

            if (PieceInfo.Player == PlayerSide.White)
            {
                SetDirection(UnitDirection.Up);
            }
            else
            {
                SetDirection(UnitDirection.Down);
            }
        }

        public virtual void SetCoords()
        {
            var newCoordinates = TransformCalculator.CalculateTransform(PieceInfo.XBoard, PieceInfo.YBoard);

            this.transform.position = new Vector3(newCoordinates.X, newCoordinates.Y, -1.0f);
        }

        protected virtual void OnMouseUp()
        {
            var game = Controller!.GetComponent<Game>();

            if (!game.IsGameOver && game.CurrentPlayer == PieceInfo.Player)
            {
                DestroyMovePlates();

                InitiateMovePlates();

                Debug.Log(this.name + " - MovePlates created");
            }
        }

        public void MoveChessPiece(PossibleMove move, UnitDirection chosenDirection)
        {
            if (move.Directions.SingleOrDefault(x => x.Direction == chosenDirection) == null)
            {
                Debug.Log("Invalid move - chosen direction is invalid.");
                return;
            }

            // if (move.RemovedChessPiece != null)
            // {
            //     var game = Controller.GetComponent<Game>();

            //     var pieceObj = game.GetChesspiece(move.End.X, move.End.Y);
            //     var chessPiece = pieceObj.GetComponent<ChessmanComponent>().ChessPieceInformation;

            //     if (chessPiece.Role == ChessPieceRole.King)
            //     {
            //         game.Winner(ChessPieceInformation.Player);
            //     }

            //     game.RemoveChesspiece(pieceObj);
            // }

            Debug.Log($"ChessmanComponent - Moving Chesspiece {PieceInfo.Player} {PieceInfo.Role}, {chosenDirection}");

            if (move.PrecedingMoves != null && move.PrecedingMoves.Count > 0)
            {
                var preMove = move.PrecedingMoves.First();
                Debug.Log($"Preceding move - {preMove.Move.End.X}, {preMove.Move.End.Y}, {preMove.Direction}");
            }

            PieceInfo.Board.MoveChessPiece(move, chosenDirection);

            SetCoords();

            SetDirection(chosenDirection);
        }

        #region Rotation

        public void SetDirection(UnitDirection direction)
        {
            var vector = DirectionConverter.Convert(direction);

            SetDirection(vector.X, vector.Y);

            //ChessPieceInformation.Direction = direction;
        }

        /// <summary>
        /// Sets direction of the piece by inputting the coordinates on the board that it wants to look at
        /// </summary>
        /// <param name="x">X coordinate on the board</param>
        /// <param name="y">Y coordinate on the board</param>
        public virtual void SetDirection(int x, int y)
        {
            // Starting Vector2 is (0, 1), because base sprite is turned upwards
            float angle = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(x, y));

            //Debug.Log($"Vector to = {x}, {y}, Rotate by {angle}");

            this.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Don't rotate the attached colored MovePlate
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

            DestroyDirectionArrows();
        }

        public void InitiateMovePlates()
        {
            var moves = PieceInfo.GetPossibleMoves();

            foreach (var move in moves)
            {
                MovePlateSpawn(move);

                //Debug.Log($"Move plate spawned - {move.End.X}, {move.End.Y}");
            }
        }

        public MovePlate MovePlateSpawn(PossibleMove possibleMove)
        {
            //var newCoordinates = CalculateTransform(possibleMove.End.X, possibleMove.End.Y);

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

        public GameObject MovePlateColoredSpawn(int x, int y, PlayerSide player)
        {
            var newCoordinates = TransformCalculator.CalculateTransform(x, y);
            GameObject mp = null;

            // -0.5f to spawn the plate below the piece, so it doesn't interfere with collision checking on click
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

        public GameObject DirectionArrowSpawn(PossibleMove move, DirectionArrow directionArrow)
        {
            var vector = DirectionConverter.Convert(directionArrow.Direction);

            var xCoords = move.End.X + vector.X / 2.8f;
            var yCoords = move.End.Y + vector.Y / 2.8f;

            //Debug.Log($"Spawn Arrow - {xCoords}, {yCoords}");

            var coordinates = TransformCalculator.CalculateTransform(xCoords, yCoords);

            GameObject arrowObj = Instantiate(DirectionArrow, new Vector3(coordinates.X, coordinates.Y, -3.5f), Quaternion.identity);
            arrowObj.GetComponent<DirectionArrowComponent>().Initialize(directionArrow);

            // float angle = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(vector.X, vector.Y));
            // arrowObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            // var arrow = arrowObj.GetComponent<DirectionArrowComponent>();
            // arrow.ArrowInfo = directionArrow;

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

