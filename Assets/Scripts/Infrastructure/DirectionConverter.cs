using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using UnityEngine;

namespace Infrastructure
{
    public static class DirectionConverter
    {
        private static Dictionary<UnitDirection, (int X, int Y)> _directionToVector2 =
            new Dictionary<UnitDirection, (int X, int Y)>()
            {
            { UnitDirection.Up, (0, 1) },
            { UnitDirection.UpperRight, (1, 1) },
            { UnitDirection.Right, (1, 0) },
            { UnitDirection.LowerRight, (1, -1) },
            { UnitDirection.Down, (0, -1) },
            { UnitDirection.LowerLeft, (-1, -1) },
            { UnitDirection.Left, (-1, 0) },
            { UnitDirection.UpperLeft, (-1, 1) },
            };

        private static Dictionary<int, UnitDirection> _angleToDirection =
            new Dictionary<int, UnitDirection>()
            {
            { 0, UnitDirection.Up },
            { 45, UnitDirection.UpperRight },
            { 90, UnitDirection.Right },
            { 135, UnitDirection.LowerRight },
            { 180, UnitDirection.Down },
            { 225, UnitDirection.LowerLeft },
            { 270, UnitDirection.Left },
            { 315, UnitDirection.UpperLeft },
            };

        private static Dictionary<UnitDirection, int> _directionToAngle = _angleToDirection.ToDictionary(p => p.Value, p => p.Key);

        public static (int X, int Y) ConvertToVector2(int angle)
        {
            var direction = ConvertAngleToDirection(angle);
            var vector2 = ConvertToVector2(direction);
            return vector2;
        }

        public static (int X, int Y) ConvertToVector2(UnitDirection direction)
        {
            if (_directionToVector2.TryGetValue(direction, out var vector2))
            {
                return vector2;
            }

            return (0, 0);
        }

        public static UnitDirection ConvertAngleToDirection(int angle)
        {
            if (_angleToDirection.TryGetValue(angle, out var direction))
            {
                return direction;
            }

            return UnitDirection.Unknown;
        }

        public static int ConvertDirectionToAngle(UnitDirection direction)
        {
            if (_directionToAngle.TryGetValue(direction, out var angle))
            {
                return angle;
            }

            return 0;
        }

        public static UnitDirection GetOppositeDirection(UnitDirection direction)
        {
            _directionToAngle.TryGetValue(direction, out var angle);
            var oppositeAngle = (angle + 180) % 360;
            _angleToDirection.TryGetValue(oppositeAngle, out var result);
            return result;
        }

        public static UnitDirection CalculateClosestDirection(Vector2 start, Vector2 end)
        {
            var baseVector2 = new Vector2(0, 1); // UP
            var directionVector2 = end - start;

            float angle = Vector2.SignedAngle(directionVector2.normalized, baseVector2);
            if (angle < 0)
            {
                angle += 360;
            }

            // round angle to the nearest multiple of 45
            int factor = 45;
            int nearestMultiple = (int)Math.Round(angle / (double)factor) * factor % 360;

            var closestDirection = DirectionConverter.ConvertAngleToDirection(nearestMultiple);

            //Debug.Log($"{directionVector2.x}, {directionVector2.y} - {angle} - {closestDirection}");

            return closestDirection;
        }

        public static bool IsAttackingSide(Chessman attacker, Chessman defender)
        {
            var directions = (UnitDirection[])Enum.GetValues(typeof(UnitDirection));

            var directionsLength = directions.Length - 1;

            directions = directions[1..directionsLength]; // Cut out 'Unknown' direction

            var attackerPosition = new Vector2(attacker.XBoard, attacker.YBoard);
            var defenderPosition = new Vector2(defender.XBoard, defender.YBoard);
            float angle = Vector2.SignedAngle(defenderPosition, attackerPosition);

            // Calculate the front 90 degree directions 
            int factor = 45;
            int attackDirection = (int)Math.Round(angle / (double)factor) % directionsLength;
            int defenderDirection = Array.IndexOf(directions, defender.Direction);
            var defenderFront = new int[] { (defenderDirection - 1) % directionsLength, defenderDirection, (defenderDirection + 1) % directionsLength };

            return !defenderFront.Contains(attackDirection);
        }
    }
}