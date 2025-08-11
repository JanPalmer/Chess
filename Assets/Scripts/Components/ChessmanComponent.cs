using System.Linq;
using Enums;
using Infrastructure;
using Models;
using TMPro;
using UnityEngine;

namespace Components
{
    public class ChessmanComponent : MonoBehaviour
    {
        // References
        public GameObject Controller { get => GameObject.FindGameObjectWithTag("GameController"); }

        private static int PieceId = 1;

        public IChessPiece PieceInfo = null!;

        public SpriteLibrary SpriteLibrary;
        private GameObject _movePlateColored;
        private Canvas _healthIndicatorCanvas;

        public Chessman ToChessman()
        {
            return new Chessman(PieceInfo);
        }

        public virtual void Activate(string spriteName, PlayerSide player, int x, int y, Board board)
        {
            SpriteLibrary = Controller.GetComponent<SpriteLibrary>();

            // take the instantianted location and adjust the transform
            this.name = spriteName + "-" + player + "-" + PieceId++;
            PieceInfo = new Chessman()
            {
                Name = this.name,
                XBoard = x,
                YBoard = y,
                Player = player,
                Role = InitializeChessPieceRole(),
                Board = board,
                Direction = (player == PlayerSide.Orange) ? UnitDirection.Right : UnitDirection.Left,
            };

            PieceInfo.SetPieceStats();

            Debug.Log(this.name);

            // set sprite
            this.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.SelectSprite(spriteName);

            SetCoords();

            var game = Controller.GetComponent<Game>();
            _movePlateColored = game.MovePlateColoredSpawn(x, y, PieceInfo.Player);
            _movePlateColored.transform.SetParent(this.transform, true);

            SetHealth();

            SetDirection(PieceInfo.Direction);
        }

        public virtual void SetCoords()
        {
            var newCoordinates = TransformCalculator.CalculateTransform(PieceInfo.XBoard, PieceInfo.YBoard);

            this.transform.position = new Vector3(newCoordinates.X, newCoordinates.Y, -1.0f);
        }

        protected virtual void OnMouseUp()
        {
            var game = Controller!.GetComponent<Game>();

            if (!game.IsGameOver
                && game.CurrentPlayer == PieceInfo.Player
                && (PieceInfo.Role != UnitRole.Wall || PieceInfo.Role != UnitRole.Unknown)
                && PieceInfo.IsReady
                && PieceInfo.IsRemoved == false)
            {
                game.HighlightedPiece = this;

                Debug.Log(this.name + " - MovePlates created");
            }
        }

        public void Move(PossibleMove move, UnitDirection chosenDirection)
        {
            if (move.Directions.SingleOrDefault(x => x.Direction == chosenDirection) == null)
            {
                Debug.Log("Invalid move - chosen direction is invalid.");
                return;
            }

            Debug.Log($"ChessmanComponent - Moving Chesspiece {PieceInfo.Player} {PieceInfo.Role}, {chosenDirection}");

            // For debugging
            if (move.PrecedingMoves != null && move.PrecedingMoves.Count > 0)
            {
                var preMove = move.PrecedingMoves.First();
                Debug.Log($"Preceding move - {preMove.Move.End.X}, {preMove.Move.End.Y}, {preMove.Direction}");
            }

            PieceInfo.Board.MoveChessPiece(move, chosenDirection);

            SetCoords();

            SetDirection(chosenDirection);
        }

        public void UndoMove(PossibleMove move)
        {
            Debug.Log($"ChessmanComponent - Undoing move Chesspiece {PieceInfo.Player} {PieceInfo.Role}, {move.StartingDirection}");

            PieceInfo.Board.UndoMove(move);

            SetCoords();

            SetDirection(move.StartingDirection);
        }

        #region Rotation

        public void SetDirection(UnitDirection direction)
        {
            var vector = DirectionConverter.ConvertToVector2(direction);
            SetDirection(vector.X, vector.Y);
        }

        public void SetHealth()
        {
            if (_healthIndicatorCanvas == null)
            {
                _healthIndicatorCanvas = this.GetComponentInChildren<Canvas>();
            }

            var text = _healthIndicatorCanvas.GetComponentInChildren<TextMeshProUGUI>();
            text.text = PieceInfo.Health.ToString();

            if (PieceInfo.IsRemoved)
            {
                var blackPieceEquivalent = "black_" + PieceInfo.Role.ToString().ToLower();
                this.GetComponent<SpriteRenderer>().sprite = SpriteLibrary.SelectSprite(blackPieceEquivalent);
                text.alpha = 0;
            }
        }

        /// <summary>
        /// Sets direction of the piece by inputting the coordinates on the board that it wants to look at
        /// </summary>
        /// <param name="x">X coordinate on the board</param>
        /// <param name="y">Y coordinate on the board</param>
        public virtual void SetDirection(int x, int y)
        {
            // Starting Vector2 is (0, 1), because base sprite is turned upwards
            this.transform.rotation = TransformCalculator.CalculateRotation(new Vector2(0, 1), new Vector2(x, y));

            // Don't rotate the attached colored MovePlate
            _movePlateColored.transform.rotation = Quaternion.identity;
            _healthIndicatorCanvas.transform.rotation = Quaternion.identity;
        }

        public virtual void UpdateReadyColor()
        {
            var plate = _movePlateColored.GetComponent<MovePlate>();

            if (PieceInfo.IsReady)
            {
                plate.SwapColor(plate.ColorBase);
            }
            else
            {
                plate.SwapColor(plate.ColorDark);
            }
        }

        #endregion

        #region Initialize

        private UnitRole InitializeChessPieceRole()
        {
            return this.name switch
            {
                string a when a.Contains("pawn") => UnitRole.Pawn,
                string a when a.Contains("bishop") => UnitRole.Bishop,
                string a when a.Contains("knight") => UnitRole.Knight,
                string a when a.Contains("rook") => UnitRole.Rook,
                string a when a.Contains("queen") => UnitRole.Queen,
                string a when a.Contains("king") => UnitRole.King,
                _ => UnitRole.Wall,
            };
        }

        #endregion
    }
}

